using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Transactions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class TransactionConfirmedConsumer : IConsumer<TransactionConfirmed>
    {
        private readonly ILogger<TransactionConfirmedConsumer> _logger;
        private readonly TransactionProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IDetectedTransactionsRepository _detectedTransactionsRepository;

        public TransactionConfirmedConsumer(ILogger<TransactionConfirmedConsumer> logger,
            TransactionProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IDepositsRepository depositsRepository,
            IDetectedTransactionsRepository detectedTransactionsRepository)
        {
            _logger = logger;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _depositsRepository = depositsRepository;
            _detectedTransactionsRepository = detectedTransactionsRepository;
        }

        public async Task Consume(ConsumeContext<TransactionConfirmed> context)
        {
            var tx = context.Message;

            var processingContext = await _processingContextBuilder.Build(
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

            if (processingContext.IsEmpty)
            {
                _logger.LogDebug("There is nothing to process in the transaction {@context}", new
                {
                    BlockchainId = tx.BlockchainId,
                    TransactionId = tx.TransactionId
                });

                return;
            }

            if (!await _detectedTransactionsRepository.Exists(tx.BlockchainId, tx.TransactionId))
            {
                if (context.GetRedeliveryCount() > 10)
                {
                    throw new InvalidOperationException($"Transaction wasn't detected yet, so confirmation can't be processed even after some tries: {tx.BlockchainId}:{tx.TransactionId}");
                }

                _logger.LogWarning("Transaction wasn't detected yet, so confirmation can't be processed. Will be retries in 30 seconds {@context}...", tx);

                await context.Redeliver(TimeSpan.FromSeconds(30));
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

            await Task.WhenAll(
                _brokerAccountsBalancesRepository.SaveAsync(
                    $"{BalanceChangingReason.TransactionConfirmed}_{tx.BlockchainId}_{tx.TransactionId}", 
                    updatedBrokerAccountBalances),
                _depositsRepository.SaveAsync(updatedDeposits));

            foreach (var evt in updatedBrokerAccountBalances.SelectMany(x => x.Events))
            {
                await context.Publish(evt);
            }

            foreach (var evt in updatedDeposits.SelectMany(x => x.Events))
            {
                await context.Publish(evt);
            }
        }
    }
}
