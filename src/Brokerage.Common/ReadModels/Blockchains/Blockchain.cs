using System;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.ReadModels.Blockchains
{
    public sealed class Blockchain
    {
        public string Id { get; set; }
        public Protocol Protocol { get; set; }
        public string TenantId { get; set; }
        public string Name { get; set; }
        public NetworkType NetworkType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public long LatestBlockNumber { get; set; }
        public long ChainSequence { get; set; }
        public int Version { get; set; }
    }
}
