using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.ReadModels.Blockchains
{
    public class Protocol
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public long StartBlockNumber { get; set; }
        public Requirements Requirements { get; set; }
        public Capabilities Capabilities { get; set; }
        public DoubleSpendingProtectionType DoubleSpendingProtectionType { get; set; }
    }
}
