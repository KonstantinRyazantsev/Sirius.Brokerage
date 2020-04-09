﻿using System.Threading.Tasks;
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

            var deposit = await _depositsRepository.GetByOperationIdAsync(evt.OperationId);

            deposit.Fail();

            await _depositsRepository.SaveAsync(deposit);

            foreach (var @event in deposit.Events)
            {
                await context.Publish(@event);
            }

            _logger.LogInformation("OperationFailed event has been processed {@context}", evt);
        }
    }
}
