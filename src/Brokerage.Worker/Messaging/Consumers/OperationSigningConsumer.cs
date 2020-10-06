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
    public class OperationSigningConsumer : IConsumer<OperationSigning>
    {
        private readonly ILogger<OperationSigningConsumer> _logger;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly OperationProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;

        public OperationSigningConsumer(ILogger<OperationSigningConsumer> logger,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            OperationProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
        }

        public async Task Consume(ConsumeContext<OperationSigning> context)
        {
            var evt = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"Operations:Signing:{evt.OperationId}");

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

                foreach (var processor in _processorsFactory.GetSigningOperationProcessors())
                {
                    await processor.Process(evt, processingContext);
                }

                var updatedWithdrawals = processingContext.Withdrawals.Where(x => x.Events.Any()).ToArray();

                await unitOfWork.Withdrawals.Update(updatedWithdrawals);

                foreach (var @event in updatedWithdrawals.SelectMany(x => x.Events))
                {
                    unitOfWork.Outbox.Publish(@event);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
