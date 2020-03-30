using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Brokerage.Bilv1.Domain.Models.Blockchains;
using Brokerage.Bilv1.Domain.Services;

namespace Brokerage.Bilv1.DomainServices
{
    public class BlockchainService : IBlockchainService
    {
        private readonly IReadOnlyDictionary<string, Blockchain> _blockchains;

        public BlockchainService()
        {
            _blockchains = new[]
            {
                new Blockchain
                {
                    Id = "Bitcoin",
                    Requirements = new BlockchainRequirements
                    {
                        Transfers = new TransfersRequirements
                        {
                            SourceAddressPublicKey = true
                        },
                        Fee = new TransactionsFeeRequirements
                        {
                            FixedFeeAssetId = default
                        }
                    },
                    Capabilities = new BlockchainCapabilities
                    {
                        TransactionsExpiration = new TransactionsExpirationCapabilities
                        {
                            AfterBlockNumber = false,
                            AfterDateTime = false
                        },
                        Transfers = new TransfersCapabilities
                        {
                            AsAtBlock = true,
                            ChangeRecipientAddress = true,
                            OneToMany = true,
                            ManyToOne = true,
                            ManyToMany = true,
                            MultipleAssets = true,
                            SourceAddressNonce = false,
                            DestinationTag = new TransfersDestinationTagCapabilities
                            {
                                Number = false,
                                Text = false
                            }
                        }
                    },
                    //Links = new BlockchainLinks
                    //{
                    //    NetworksUrl = Url.NetworksUrl("Bitcoin")
                    //}
                },
                new Blockchain
                {
                    Id = "Ethereum",
                    Requirements = new BlockchainRequirements
                    {
                        Transfers = new TransfersRequirements
                        {
                            SourceAddressPublicKey = false
                        },
                        Fee = new TransactionsFeeRequirements
                        {
                            FixedFeeAssetId = "ETH"
                        }
                    },
                    Capabilities = new BlockchainCapabilities
                    {
                        TransactionsExpiration = new TransactionsExpirationCapabilities
                        {
                            AfterBlockNumber = false,
                            AfterDateTime = false
                        },
                        Transfers = new TransfersCapabilities
                        {
                            AsAtBlock = false,
                            ChangeRecipientAddress = false,
                            OneToMany = false,
                            ManyToOne = false,
                            ManyToMany = false,
                            MultipleAssets = false,
                            SourceAddressNonce = true,
                            DestinationTag = new TransfersDestinationTagCapabilities
                            {
                                Number = false,
                                Text = false
                            }
                        }
                    },
                    //Links = new BlockchainLinks
                    //{
                    //    NetworksUrl = Url.NetworksUrl("Ethereum")
                    //}
                },
                new Blockchain
                {
                    Id = "Stellar",
                    Requirements = new BlockchainRequirements
                    {
                        Transfers = new TransfersRequirements
                        {
                            SourceAddressPublicKey = false
                        },
                        Fee = new TransactionsFeeRequirements
                        {
                            FixedFeeAssetId = "XLM"
                        }
                    },
                    Capabilities = new BlockchainCapabilities
                    {
                        TransactionsExpiration = new TransactionsExpirationCapabilities
                        {
                            AfterBlockNumber = false,
                            AfterDateTime = true
                        },
                        Transfers = new TransfersCapabilities
                        {
                            AsAtBlock = false,
                            ChangeRecipientAddress = false,
                            OneToMany = true,
                            ManyToOne = true,
                            ManyToMany = true,
                            MultipleAssets = true,
                            SourceAddressNonce = true,
                            DestinationTag = new TransfersDestinationTagCapabilities
                            {
                                Number = true,
                                Text = true,
                                MinNumber = 0,
                                MaxNumber = BigInteger.Parse("18446744073709551615"),
                                MaxTextLength = 28,
                                NumberTagNames = new Dictionary<string, string>
                                {
                                    ["En-en"] = "Memo ID"
                                },
                                TextTagNames = new Dictionary<string, string>
                                {
                                    ["En-en"] = "Memo text"
                                }
                            }
                        }
                    },
                    //Links = new BlockchainLinks
                    //{
                    //    NetworksUrl = Url.NetworksUrl("Stellar")
                    //}
                }
            }.ToDictionary(x => x.Id);
        }

        public IReadOnlyCollection<Blockchain> GetAllBlockchains()
        {
            return _blockchains.Values.ToImmutableArray();
        }

        public Blockchain GetBlockchainById(string blockchainId)
        {
            _blockchains.TryGetValue(blockchainId, out var blockchain);

            return blockchain;
        }
    }
}
