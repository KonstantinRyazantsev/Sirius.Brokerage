using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Transactions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionConfirmedConsumer : IConsumer<TransactionConfirmed>
    {
        private readonly ILogger<TransactionConfirmedConsumer> _logger;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IDetectedTransactionsRepository _detectedTransactionsRepository;
        private readonly IOutboxManager _outboxManager;

        public TransactionConfirmedConsumer(ILogger<TransactionConfirmedConsumer> logger,
            IProcessorsFactory processorsFactory,
            IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IDepositsRepository depositsRepository,
            IOperationsRepository operationsRepository,
            IDetectedTransactionsRepository detectedTransactionsRepository,
            IOutboxManager outboxManager)
        {
            _logger = logger;
            _processorsFactory = processorsFactory;
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _depositsRepository = depositsRepository;
            _operationsRepository = operationsRepository;
            _detectedTransactionsRepository = detectedTransactionsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<TransactionConfirmed> context)
        {
            var tx = context.Message;

            if (!await _detectedTransactionsRepository.Exists(tx.BlockchainId, tx.TransactionId))
            {
                _logger.LogWarning("Transaction wasn't detected yet, so confirmation can't be processed {@context}. Waiting 10 seconds before retry...", tx);

                // TODO: Remove this hack
                await Task.Delay(TimeSpan.FromSeconds(10));

                throw new InvalidOperationException($"Transaction wasn't detected yet, so confirmation can't be processed: {tx.BlockchainId}:{tx.TransactionId}");
            }

            var processingContextBuilder = new TransactionProcessingContextBuilder(
                _accountRequisitesRepository, 
                _brokerAccountRequisitesRepository, 
                _brokerAccountsBalancesRepository,
                _depositsRepository,
                _operationsRepository, 
                _outboxManager);
            var processingContext = await processingContextBuilder.Build(
                tx.BlockchainId,
                tx.OperationId,
                // TODO: Add timestamp to the tx event
                new TransactionInfo(tx.TransactionId, tx.BlockNumber, tx.RequiredConfirmationsCount, DateTime.UtcNow),
                tx.Sources
                    .Select(x => new SourceContext(x.Address, x.Unit))
                    .ToArray(),
                tx.Destinations
                    .Select(x => new DestinationContext(
                        x.Address,
                        x.Tag,
                        x.TagType,
                        x.Unit))
                    .ToArray());

            // TODO: Hack ignore incoming transaction to the broker accounts generated from the BIL v1 balances
            if (tx.TransactionId.Length < 5 && processingContext.BrokerAccounts.SelectMany(x => x.Inputs).Any())
            {
                _logger.LogInformation("There is a BIL v1 transaction to a broker account based on balances. It will be skipped to avoid duplication {@context}", tx);

                return;
            }

            if (processingContext.IsEmpty)
            {
                _logger.LogInformation("There is nothing to process in the transaction {@context}", tx);

                return;
            }

            foreach (var processor in _processorsFactory.GetConfirmedTransactionProcessors())
            {
                await processor.Process(tx, processingContext);
            }

            var updatedBrokerAccountBalances = processingContext.BrokerAccounts
                .SelectMany(x => x.Balances.Select(b => b.Balances))
                .Where(x => x.Events.Any())
                .ToArray();
            var updatedDeposits = processingContext.Deposits
                .Where(x => x.Events.Any())
                .ToArray();

            // TODO: Use Sequence instead of the update ID for the balances
            await Task.WhenAll(
                _brokerAccountsBalancesRepository.SaveAsync($"{tx.TransactionId}_{TransactionStage.Confirmed}", updatedBrokerAccountBalances),
                _depositsRepository.SaveAsync(updatedDeposits));

            foreach (var evt in updatedBrokerAccountBalances.SelectMany(x => x.Events))
            {
                await context.Publish(evt);
            }

            foreach (var evt in updatedDeposits.SelectMany(x => x.Events))
            {
                await context.Publish(evt);
            }

            _logger.LogInformation("Confirmed transaction has been processed {@context}", tx);
        }
    }
}
