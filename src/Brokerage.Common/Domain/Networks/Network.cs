using System;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Networks
{
    public class Network
    {
        public NetworkId NetworkId { get; set; }
        public BlockchainId BlockchainId { get; set; }
    }
}
