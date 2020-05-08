using System.Threading.Tasks;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.ReadModels.Blockchains;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract.Blockchains;

namespace Brokerage.Worker.MessageConsumers
{
    public class BlockchainUpdatedConsumer : IConsumer<BlockchainUpdated>
    {
        private readonly ILogger<BlockchainUpdatedConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;

        public BlockchainUpdatedConsumer(
            ILogger<BlockchainUpdatedConsumer> logger,
            IBlockchainsRepository blockchainsRepository)
        {
            _logger = logger;
            this._blockchainsRepository = blockchainsRepository;
        }

        public async Task Consume(ConsumeContext<BlockchainUpdated> context)
        {
            var evt = context.Message;

            var model = new Blockchain
            {
                Id = evt.BlockchainId,
            };

            await _blockchainsRepository.AddOrReplaceAsync(model);

            _logger.LogInformation("BlockchainUpdated command has been processed {@context}", evt);
        }
    }
}
