using Newtonsoft.Json;

namespace Brokerage.Bilv1.Domain.Models.Blockchains
{
    public sealed class Blockchain
    {
        public string Id { get; set; }
        public BlockchainCapabilities Capabilities { get; set; }
        public BlockchainRequirements Requirements { get; set; }

        // TODO: This should be in the response contract only
        [JsonProperty(PropertyName = "_links")]
        public BlockchainLinks Links { get; set; }
    }

    public sealed class BlockchainLinks
    {
        public string NetworksUrl { get; set; }
    }
}
