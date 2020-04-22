using System.Threading.Tasks;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.ReadModels.Blockchains;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class BlockchainUpdatesConsumer : IConsumer<BlockchainAdded>
    {
        private readonly ILogger<BlockchainUpdatesConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;

        public BlockchainUpdatesConsumer(
            ILogger<BlockchainUpdatesConsumer> logger, 
            IBlockchainsRepository blockchainsRepository)
        {
            _logger = logger;
            this._blockchainsRepository = blockchainsRepository;
        }

        public async Task Consume(ConsumeContext<BlockchainAdded> context)
        {
            var evt = context.Message;

            var model = new Blockchain
            {
                Id = evt.BlockchainId,
            };

            await _blockchainsRepository.AddOrReplaceAsync(model);

            _logger.LogInformation("BlockchainAdded command has been processed {@context}", evt);
        }
    }
}
