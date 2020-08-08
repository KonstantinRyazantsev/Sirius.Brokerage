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
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class TransactionDetectedConsumer : IConsumer<TransactionDetected>
    {
        private readonly ILogger<TransactionDetectedConsumer> _logger;
        private readonly TransactionProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IDetectedTransactionsRepository _detectedTransactionsRepository;

        public TransactionDetectedConsumer(ILogger<TransactionDetectedConsumer> logger,
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

        public async Task Consume(ConsumeContext<TransactionDetected> context)
        {
            var tx = context.Message;

            var processingContext = await _processingContextBuilder.Build(
                tx.BlockchainId,
                tx.OperationId,
                new TransactionInfo(tx.TransactionId, tx.BlockNumber, -1, tx.BlockMinedAt),
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
                    TransactionId = tx.TransactionId,
                    OperationId = tx.OperationId
                });

                return;
            }

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

            await Task.WhenAll(
                _brokerAccountsBalancesRepository.SaveAsync(
                    $"{BalanceChangingReason.TransactionDetected}_{tx.BlockchainId}_{tx.TransactionId}", 
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

            await _detectedTransactionsRepository.AddOrIgnore(tx.BlockchainId, tx.TransactionId);
        }
    }
}
