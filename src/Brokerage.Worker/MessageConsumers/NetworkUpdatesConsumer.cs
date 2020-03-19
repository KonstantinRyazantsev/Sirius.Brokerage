using System.Threading.Tasks;
using Brokerage.Common.Domain.Networks;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class NetworkUpdatesConsumer : IConsumer<NetworkAdded>
    {
        private readonly ILogger<NetworkUpdatesConsumer> _logger;
        private readonly INetworkReadModelRepository _networkReadModelRepository;

        public NetworkUpdatesConsumer(
            ILogger<NetworkUpdatesConsumer> logger,
            INetworkReadModelRepository networkReadModelRepository)
        {
            _logger = logger;
            _networkReadModelRepository = networkReadModelRepository;
        }

        public async Task Consume(ConsumeContext<NetworkAdded> context)
        {
            var @event = context.Message;

            var model = new Network()
            {
                BlockchainId = context.Message.BlockchainId,
                NetworkId = context.Message.NetworkId
            };

            await _networkReadModelRepository.AddOrReplaceAsync(model);

            _logger.LogInformation("NetworkAdded command has been processed {@message}", @event);

            await Task.CompletedTask;
        }
    }
}
