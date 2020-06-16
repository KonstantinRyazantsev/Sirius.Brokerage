using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.ReadModels.Blockchains
{
    public class Protocol
    {
        public string Code { get; set; }
        public Capabilities Capabilities { get; set; }
    }
}
