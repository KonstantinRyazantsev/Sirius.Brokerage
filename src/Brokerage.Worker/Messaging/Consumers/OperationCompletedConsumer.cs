using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class OperationCompletedConsumer : IConsumer<OperationCompleted>
    {
        private readonly ILogger<OperationCompletedConsumer> _logger;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly OperationProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;

        public OperationCompletedConsumer(ILogger<OperationCompletedConsumer> logger,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            OperationProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
        }

        public async Task Consume(ConsumeContext<OperationCompleted> context)
        {
            var evt = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"Operations:Completed:{evt.OperationId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var processingContext = await _processingContextBuilder.Build(
                    evt.OperationId,
                    unitOfWork.Operations,
                    unitOfWork.Deposits,
                    unitOfWork.BrokerAccountBalances,
                    unitOfWork.Withdrawals,
                    unitOfWork.MinDepositResiduals);
                
                if (processingContext.IsEmpty)
                {
                    _logger.LogInformation("There is nothing to process in the operation {@context}", evt);

                    return;
                }

                foreach (var processor in _processorsFactory.GetCompletedOperationProcessors())
                {
                    await processor.Process(evt, processingContext);
                }

                var updatedDeposits = processingContext.RegularDeposits.Where(x => x.Events.Any()).ToArray();
                var updatedMinDeposits = processingContext.TinyDeposits.Where(x => x.Events.Any()).ToArray();
                var minDepositResiduals = processingContext.MinDepositResiduals;
                var updatedWithdrawals = processingContext.Withdrawals.Where(x => x.Events.Any()).ToArray();
                var updatedBrokerAccountBalances = processingContext.BrokerAccountBalances.Values.Where(x => x.Events.Any()).ToArray();
                var operation = processingContext.Operation;

                // TODO: Complete operation
                operation.AddActualFees(evt.ActualFees);

                await unitOfWork.Deposits.Save(updatedDeposits);
                await unitOfWork.Deposits.Save(updatedMinDeposits);
                await unitOfWork.Withdrawals.Update(updatedWithdrawals);
                await unitOfWork.BrokerAccountBalances.Save(updatedBrokerAccountBalances);
                await unitOfWork.Operations.Update(operation);
                await unitOfWork.MinDepositResiduals.Remove(minDepositResiduals);

                foreach (var @event in updatedDeposits.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(@event);
                }

                foreach (var @event in updatedMinDeposits.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(@event);
                }

                foreach (var @event in updatedWithdrawals.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(@event);
                }

                foreach (var @event in updatedBrokerAccountBalances.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(@event);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
