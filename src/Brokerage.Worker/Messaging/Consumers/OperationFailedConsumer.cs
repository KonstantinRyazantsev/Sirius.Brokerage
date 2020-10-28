using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Blockchains;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class OperationFailedConsumer : IConsumer<OperationFailed>
    {
        private readonly ILogger<OperationFailedConsumer> _logger;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly OperationProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IBlockchainsRepository _blockchainsRepository;

        public OperationFailedConsumer(ILogger<OperationFailedConsumer> logger,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            OperationProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory,
            IBlockchainsRepository blockchainsRepository)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
            _blockchainsRepository = blockchainsRepository;
        }

        public async Task Consume(ConsumeContext<OperationFailed> context)
        {
            var evt = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"Operations:Failed:{evt.OperationId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var processingContext = await _processingContextBuilder.Build(evt.OperationId,
                    unitOfWork.Operations,
                    unitOfWork.Deposits,
                    unitOfWork.BrokerAccountBalances,
                    unitOfWork.Withdrawals,
                    unitOfWork.MinDepositResiduals,
                    _blockchainsRepository);

                if (processingContext.IsEmpty)
                {
                    _logger.LogInformation("There is nothing to process in the operation {@context}", evt);

                    return;
                }

                foreach (var processor in _processorsFactory.GetFailedOperationProcessors())
                {
                    await processor.Process(evt, processingContext);
                }

                var updatedDeposits = processingContext.Deposits.Where(x => x.Events.Any()).ToArray();
                var updatedWithdrawals = processingContext.Withdrawals.Where(x => x.Events.Any()).ToArray();
                var updatedBrokerAccountBalances = processingContext.BrokerAccountBalances.Values.Where(x => x.Events.Any()).ToArray();
                var operation = processingContext.Operation;

                // TODO: Fail operation
                operation.AddActualFees(evt.ActualFees);

                await unitOfWork.Deposits.Save(updatedDeposits);
                await unitOfWork.Withdrawals.Update(updatedWithdrawals);
                await unitOfWork.BrokerAccountBalances.Save(updatedBrokerAccountBalances);
                await unitOfWork.Operations.Update(operation);

                foreach (var @event in updatedDeposits.SelectMany(x => x.Events))
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
