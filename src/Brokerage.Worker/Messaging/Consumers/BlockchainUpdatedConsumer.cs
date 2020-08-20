using System.Threading.Tasks;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.ReadModels.Blockchains;
using MassTransit;
using Swisschain.Sirius.Integrations.MessagingContract.Blockchains;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class BlockchainUpdatedConsumer : IConsumer<BlockchainUpdated>
    {
        private readonly IBlockchainsRepository _blockchainsRepository;

        public BlockchainUpdatedConsumer(IBlockchainsRepository blockchainsRepository)
        {
            _blockchainsRepository = blockchainsRepository;
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
        }
    }
}
