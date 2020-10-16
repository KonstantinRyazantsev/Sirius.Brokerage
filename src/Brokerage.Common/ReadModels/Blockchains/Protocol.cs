using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.ReadModels.Blockchains
{
    public class Protocol
    {
        public string Code { get; set; }
        public Capabilities Capabilities { get; set; }

        public BlockchainAssetId FeePayingAssetId { get; set; }

        public long FeePayingSiriusAssetId { get; set; }
    }
}
