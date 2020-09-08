using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class TransactionConfirmedConsumer : IConsumer<TransactionConfirmed>
    {
        private readonly ILogger<TransactionConfirmedConsumer> _logger;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly TransactionProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;

        public TransactionConfirmedConsumer(ILogger<TransactionConfirmedConsumer> logger,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            TransactionProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
        }

        public async Task Consume(ConsumeContext<TransactionConfirmed> context)
        {
            var tx = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"Transactions:Confirmed:{tx.BlockchainId}:{tx.TransactionId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var processingContext = await _processingContextBuilder.Build(
                    tx.BlockchainId,
                    tx.OperationId,
                    // TODO: Add timestamp to the tx event
                    new TransactionInfo(tx.TransactionId,
                        tx.BlockNumber,
                        tx.RequiredConfirmationsCount,
                        DateTime.UtcNow),
                    tx.Sources
                        .Select(x => new SourceContext(x.Address, x.Unit))
                        .ToArray(),
                    tx.Destinations
                        .Select(x => new DestinationContext(
                            x.Address,
                            x.Tag,
                            x.TagType,
                            x.Unit))
                        .ToArray(),
                    unitOfWork.BrokerAccounts,
                    unitOfWork.AccountDetails,
                    unitOfWork.BrokerAccountDetails,
                    unitOfWork.BrokerAccountBalances,
                    unitOfWork.Deposits,
                    unitOfWork.Operations,
                    unitOfWork.MinDepositResiduals);

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

                if (!await unitOfWork.DetectedTransactions.Exists(tx.BlockchainId, tx.TransactionId))
                {
                    // TODO: Save to DB and process later.

                    _logger.LogWarning(
                        "Transaction wasn't detected yet, so confirmation can't be processed. {@context}...",
                        tx);

                    throw new InvalidOperationException($"Transaction wasn't detected yet, so confirmation can't be processed: {tx.BlockchainId}:{tx.TransactionId}");
                }

                foreach (var processor in _processorsFactory.GetConfirmedTransactionProcessors())
                {
                    await processor.Process(tx, processingContext);
                }

                var newMinDepositResiduals = processingContext.NewMinDepositResiduals;
                var prevMinDepositResiduals = processingContext.MinDepositResiduals;
                var updatedBrokerAccountBalances = processingContext.BrokerAccounts
                    .SelectMany(x => x.Balances.Select(b => b.Balances))
                    .Where(x => x.Events.Any())
                    .ToArray();
                var updatedDeposits = processingContext.Deposits
                    .Where(x => x.Events.Any())
                    .ToArray();

                await unitOfWork.BrokerAccountBalances.Save(updatedBrokerAccountBalances);
                await unitOfWork.Deposits.Save(updatedDeposits);
                await unitOfWork.Operations.Add(processingContext.NewOperations);
                await unitOfWork.MinDepositResiduals.Save(newMinDepositResiduals);
                await unitOfWork.MinDepositResiduals.Save(prevMinDepositResiduals);

                foreach (var evt in updatedBrokerAccountBalances.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(evt);
                }

                foreach (var evt in updatedDeposits.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(evt);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
