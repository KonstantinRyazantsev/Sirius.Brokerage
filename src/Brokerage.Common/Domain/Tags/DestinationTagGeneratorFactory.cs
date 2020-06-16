using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Configuration;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.ReadModels.Blockchains;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Tags
{
    public class DestinationTagGeneratorFactory : IDestinationTagGeneratorFactory
    {
        private readonly Dictionary<string, BlockchainConfig> _blockchains;

        public DestinationTagGeneratorFactory(AppConfig appConfig)
        {
            _blockchains = appConfig.Blockchains?.Blockchains.ToDictionary(x => x.ProtocolCode) ?? new Dictionary<string, BlockchainConfig>();
        }

        public IDestinationTagGenerator Create(Blockchain blockchain, DestinationTagType tagType)
        {
            switch (tagType)
            {
                case DestinationTagType.Number:
                    if (blockchain.Protocol.Capabilities.DestinationTag.Number != null)
                    {
                        _blockchains.TryGetValue(blockchain.Protocol.Code, out var config);
                        return new NumberDestinationTagGenerator(blockchain.Protocol.Capabilities.DestinationTag.Number, config);
                    }

                    return null;

                case DestinationTagType.Text:
                    if (blockchain.Protocol.Capabilities.DestinationTag.Text != null)
                    {
                        _blockchains.TryGetValue(blockchain.Protocol.Code, out var config);
                        return new TextDestinationTagGenerator(blockchain.Protocol.Capabilities.DestinationTag.Text, config);
                    }

                    return null;
                default:
                    return null;
            }
        }
    }
}
