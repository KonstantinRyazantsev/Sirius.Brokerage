using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Entities;
using Brokerage.Common.Persistence.Operations;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionDetectedConsumer : IConsumer<TransactionDetected>
    {
        private readonly ILogger<TransactionDetectedConsumer> _logger;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IOutboxManager _outboxManager;

        public TransactionDetectedConsumer(ILogger<TransactionDetectedConsumer> logger,
            IProcessorsFactory processorsFactory,
            IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IDepositsRepository depositsRepository,
            IOperationsRepository operationsRepository,
            IOutboxManager outboxManager)
        {
            _logger = logger;
            _processorsFactory = processorsFactory;
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _depositsRepository = depositsRepository;
            _operationsRepository = operationsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<TransactionDetected> context)
        {
            var tx = context.Message;
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
                // TODO: Required confirmations count
                // TODO: Add timestamp to the tx event
                new TransactionInfo(tx.TransactionId, tx.BlockNumber, -1, DateTime.UtcNow),
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

            foreach (var processor in _processorsFactory.GetDetectedTransactionProcessors())
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
                _brokerAccountsBalancesRepository.SaveAsync($"{tx.TransactionId}_{TransactionStage.Detected}", updatedBrokerAccountBalances),
                _depositsRepository.SaveAsync(updatedDeposits));

            foreach (var evt in updatedBrokerAccountBalances.SelectMany(x => x.Events))
            {
                await context.Publish(evt);
            }

            foreach (var evt in updatedDeposits.SelectMany(x => x.Events))
            {
                await context.Publish(evt);
            }

            _logger.LogInformation("Detected transaction has been processed {@context}", tx);
        }
    }
}
