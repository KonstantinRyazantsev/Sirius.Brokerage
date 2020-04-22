using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public sealed class BrokerAccountBalancesId : IEquatable<BrokerAccountBalancesId>
    {
        public BrokerAccountBalancesId(long brokerAccountId, long assetId)
        {
            BrokerAccountId = brokerAccountId;
            AssetId = assetId;
        }

        public long BrokerAccountId { get; }
        public long AssetId { get; }

        public bool Equals(BrokerAccountBalancesId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BrokerAccountId == other.BrokerAccountId && AssetId == other.AssetId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is BrokerAccountBalancesId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BrokerAccountId, AssetId);
        }

        public override string ToString()
        {
            return $"{BrokerAccountId}-{AssetId}";
        }
    }
}
