using System;
using System.Collections.Generic;
using Brokerage.Common.Configuration;
using Brokerage.Common.ReadModels.Blockchains;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Tags
{
    public class DestinationTagGeneratorFactory : IDestinationTagGeneratorFactory
    {
        private readonly IReadOnlyDictionary<string, BlockchainProtocolConfig> _blockchains;

        public DestinationTagGeneratorFactory(AppConfig appConfig)
        {
            _blockchains = appConfig.BlockchainProtocols;
        }

        public IDestinationTagGenerator CreateOrDefault(Blockchain blockchain)
        {
            if (!_blockchains.TryGetValue(blockchain.Protocol.Code, out var config))
                return null;

            var tagType = config.DestinationTagType;

            switch (tagType)
            {
                case DestinationTagType.Number:
                    if (blockchain.Protocol.Capabilities.DestinationTag.Number != null)
                    {
                        return new NumberDestinationTagGenerator(blockchain.Protocol.Capabilities.DestinationTag.Number, config);
                    }

                    return null;

                case DestinationTagType.Text:
                    if (blockchain.Protocol.Capabilities.DestinationTag.Text != null)
                    {
                        return new TextDestinationTagGenerator(blockchain.Protocol.Capabilities.DestinationTag.Text, config);
                    }

                    return null;
                default:
                    return null;
            }
        }

        public IDestinationTagGenerator Create(Blockchain blockchain)
        {
            return this.CreateOrDefault(blockchain) ??
                   throw new ArgumentException($"Tag generation is not supported for {blockchain.Id}", nameof(blockchain));
        }
    }
}
