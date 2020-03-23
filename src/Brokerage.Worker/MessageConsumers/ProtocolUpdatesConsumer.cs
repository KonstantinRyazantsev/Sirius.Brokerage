using System.Threading.Tasks;
using Brokerage.Common.Domain.Protocols;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class ProtocolUpdatesConsumer : IConsumer<ProtocolAdded>
    {
        private readonly ILogger<ProtocolUpdatesConsumer> _logger;
        private readonly IProtocolReadModelRepository _protocolReadModelRepository;

        public ProtocolUpdatesConsumer(
            ILogger<ProtocolUpdatesConsumer> logger,
            IProtocolReadModelRepository protocolReadModelRepository)
        {
            _logger = logger;
            _protocolReadModelRepository = protocolReadModelRepository;
        }

        public async Task Consume(ConsumeContext<ProtocolAdded> context)
        {
            var @event = context.Message;

            var model = new Protocol()
            {
                ProtocolId = @event.ProtocolId
            };

            await _protocolReadModelRepository.AddOrReplaceAsync(model);

            _logger.LogInformation("ProtocolAdded command has been processed {@message}", @event);

            await Task.CompletedTask;
        }
    }
}
