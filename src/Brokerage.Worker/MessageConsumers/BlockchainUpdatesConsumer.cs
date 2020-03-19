using System.Threading.Tasks;
using Brokerage.Common.Domain.Blockchains;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class BlockchainUpdatesConsumer : IConsumer<BlockchainAdded>
    {
        private readonly ILogger<BlockchainUpdatesConsumer> _logger;
        private readonly IBlockchainReadModelRepository _blockchainReadModelRepository;

        public BlockchainUpdatesConsumer(
            ILogger<BlockchainUpdatesConsumer> logger, 
            IBlockchainReadModelRepository blockchainReadModelRepository)
        {
            _logger = logger;
            _blockchainReadModelRepository = blockchainReadModelRepository;
        }

        public async Task Consume(ConsumeContext<BlockchainAdded> context)
        {
            var @event = context.Message;

            var model = new Blockchain()
            {
                BlockchainId = context.Message.BlockchainId
            };

            await _blockchainReadModelRepository.AddOrReplaceAsync(model);

            _logger.LogInformation("BlockchainAdded command has been processed {@message}", @event);

            await Task.CompletedTask;
        }
    }
}
