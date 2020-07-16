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
            var blockchain = new Blockchain
            {
                Id = evt.BlockchainId,
                Protocol = new Common.ReadModels.Blockchains.Protocol
                {
                    Code = evt.Protocol.Code,
                    Capabilities = new Common.ReadModels.Blockchains.Capabilities
                    {
                        DestinationTag = evt.Protocol.Capabilities.DestinationTag == null
                                ? null
                                : new Common.ReadModels.Blockchains.DestinationTagCapabilities
                                {
                                    Text =
                                        evt.Protocol.Capabilities.DestinationTag.Text == null
                                            ? null
                                            : new Common.ReadModels.Blockchains.TextDestinationTagsCapabilities
                                            {
                                                MaxLength = evt.Protocol.Capabilities.DestinationTag.Text.MaxLength
                                            },
                                    Number = evt.Protocol.Capabilities.DestinationTag.Number == null
                                        ? null
                                        : new Common.ReadModels.Blockchains.NumberDestinationTagsCapabilities
                                        {
                                            Max = evt.Protocol.Capabilities.DestinationTag.Number.Max,
                                            Min = evt.Protocol.Capabilities.DestinationTag.Number.Min
                                        }
                                }
                    },
                },
                CreatedAt = evt.CreatedAt,
                NetworkType = evt.NetworkType,
                UpdatedAt = evt.UpdatedAt
            };

            await _blockchainsRepository.AddOrReplaceAsync(blockchain);

            _logger.LogInformation("Blockchain has been updated {@context}", evt);
        }
    }
}
