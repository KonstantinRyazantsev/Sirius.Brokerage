using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence.Deposits;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class OperationFailedConsumer : IConsumer<OperationFailed>
    {
        private readonly ILogger<OperationFailedConsumer> _logger;
        private readonly IDepositsRepository _depositsRepository;

        public OperationFailedConsumer(
            ILogger<OperationFailedConsumer> logger,
            IDepositsRepository depositsRepository)
        {
            _logger = logger;
            _depositsRepository = depositsRepository;
        }

        public async Task Consume(ConsumeContext<OperationFailed> context)
        {
            var evt = context.Message;

            var deposit = await _depositsRepository.GetByConsolidationOperationIdAsync(evt.OperationId);

            deposit.Fail(new DepositError(evt.ErrorMessage, evt.ErrorCode switch
            {
                OperationErrorCode.TechnicalProblem =>          DepositErrorCode.TechnicalProblem,
                OperationErrorCode.NotEnoughBalance =>          DepositErrorCode.TechnicalProblem,
                OperationErrorCode.InvalidDestinationAddress => DepositErrorCode.TechnicalProblem,
                OperationErrorCode.DestinationTagRequired =>    DepositErrorCode.TechnicalProblem,
                OperationErrorCode.AmountIsTooSmall =>          DepositErrorCode.TechnicalProblem,
                 
                _ => throw new ArgumentOutOfRangeException(nameof(evt.ErrorCode), evt.ErrorCode, null)
            }));

            await _depositsRepository.SaveAsync(deposit);

            foreach (var @event in deposit.Events)
            {
                await context.Publish(@event);
            }

            _logger.LogInformation("OperationFailed event has been processed {@context}", evt);
        }
    }
}
