using System.Threading.Tasks;
using Brokerage.Common.Persistence.Deposits;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class OperationCompletedConsumer : IConsumer<OperationCompleted>
    {
        private readonly ILogger<OperationCompletedConsumer> _logger;
        private readonly IDepositsRepository _depositsRepository;

        public OperationCompletedConsumer(
            ILogger<OperationCompletedConsumer> logger,
            IDepositsRepository depositsRepository)
        {
            _logger = logger;
            _depositsRepository = depositsRepository;
        }

        public async Task Consume(ConsumeContext<OperationCompleted> context)
        {
            var evt = context.Message;

            var deposit =  await _depositsRepository.GetByConsolidationOperationIdAsync(evt.OperationId);

            deposit.Complete();

            await _depositsRepository.SaveAsync(deposit);

            foreach (var @event in deposit.Events)
            {
                await context.Publish(@event);
            }

            _logger.LogInformation("OperationCompleted event has been processed {@context}", evt);
        }
    }
}
