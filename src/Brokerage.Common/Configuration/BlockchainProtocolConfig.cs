using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Configuration
{
    public class BlockchainProtocolConfig
    {
        public string ProtocolCode { get; set; }

        public long? DesiredTextTagLength { get; set; }

        public long? DesiredMaxNumberTag { get; set; }

        public DestinationTagType DestinationTagType { get; set; }
    }
}
