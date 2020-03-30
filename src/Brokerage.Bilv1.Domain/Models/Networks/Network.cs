using System.Text.Json.Serialization;

namespace Brokerage.Bilv1.Domain.Models.Networks
{
    public sealed class Network
    {
        public string Id { get; set; }
        public string Name { get; set; } 
        public NetworkType Type { get; set; }

        public string WalletManagerUrl { get; set; }

        // TODO: This should be in the response model 
        [JsonPropertyName("_links")]
        public NetworkLinks Links { get; set; }
    }

    public sealed class NetworkLinks
    {
        public string AssetsUrl { get; set; }
        public string WalletsUrl { get; set; }
    }
}
