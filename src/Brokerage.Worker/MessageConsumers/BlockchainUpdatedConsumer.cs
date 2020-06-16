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
            var blockchain = await _blockchainsRepository.GetOrDefaultAsync(evt.BlockchainId);

            if (blockchain == null)
            {
                blockchain = new Blockchain
                {
                    Id = evt.BlockchainId,
                    Name = evt.Name,
                    NetworkType = evt.NetworkType,
                    Protocol = new Common.ReadModels.Blockchains.Protocol
                    {
                        Code = evt.Protocol.Code,
                        Name = evt.Protocol.Name,
                        StartBlockNumber = evt.Protocol.StartBlockNumber,
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
                                                Name = evt.Protocol.Capabilities.DestinationTag.Text.Name,
                                                MaxLength = evt.Protocol.Capabilities.DestinationTag.Text.MaxLength
                                            },
                                    Number = evt.Protocol.Capabilities.DestinationTag.Number == null
                                        ? null
                                        : new Common.ReadModels.Blockchains.NumberDestinationTagsCapabilities
                                        {
                                            Name = evt.Protocol.Capabilities.DestinationTag.Number.Name,
                                            Max = evt.Protocol.Capabilities.DestinationTag.Number.Max,
                                            Min = evt.Protocol.Capabilities.DestinationTag.Number.Min
                                        }
                                }
                        },
                        DoubleSpendingProtectionType = evt.Protocol.DoubleSpendingProtectionType,
                        Requirements = new Common.ReadModels.Blockchains.Requirements
                        {
                            PublicKey = evt.Protocol.Requirements.PublicKey
                        }
                    },
                    TenantId = evt.TenantId,
                    CreatedAt = evt.CreatedAt,
                    UpdatedAt = evt.CreatedAt,
                    ChainSequence = -1
                };

                await _blockchainsRepository.Add(blockchain);
            }
            else
            {
                if (blockchain.UpdatedAt.UtcDateTime < evt.UpdatedAt)
                {
                    blockchain.Name = evt.Name;
                    blockchain.NetworkType = evt.NetworkType;
                    blockchain.Protocol = new Common.ReadModels.Blockchains.Protocol
                    {
                        Code = evt.Protocol.Code,
                        Name = evt.Protocol.Name,
                        StartBlockNumber = evt.Protocol.StartBlockNumber,
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
                                                Name = evt.Protocol.Capabilities.DestinationTag.Text.Name,
                                                MaxLength = evt.Protocol.Capabilities.DestinationTag.Text
                                                    .MaxLength
                                            },
                                    Number = evt.Protocol.Capabilities.DestinationTag.Number == null
                                        ? null
                                        : new Common.ReadModels.Blockchains.NumberDestinationTagsCapabilities
                                        {
                                            Name = evt.Protocol.Capabilities.DestinationTag.Number.Name,
                                            Max = evt.Protocol.Capabilities.DestinationTag.Number.Max,
                                            Min = evt.Protocol.Capabilities.DestinationTag.Number.Min
                                        }
                                }
                        },
                        DoubleSpendingProtectionType = evt.Protocol.DoubleSpendingProtectionType,
                        Requirements = new Common.ReadModels.Blockchains.Requirements
                        {
                            PublicKey = evt.Protocol.Requirements.PublicKey
                        }
                    };
                    blockchain.TenantId = evt.TenantId;
                    blockchain.CreatedAt = evt.CreatedAt;
                    blockchain.UpdatedAt = evt.UpdatedAt;

                    await _blockchainsRepository.Update(blockchain);
                }
            }

            _logger.LogInformation("Blockchain has been updated {@context}", evt);
        }
    }
}
