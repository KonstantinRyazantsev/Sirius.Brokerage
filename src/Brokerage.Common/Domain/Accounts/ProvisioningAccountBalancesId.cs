using System;

namespace Brokerage.Common.Domain.Accounts
{
    public sealed class ProvisioningAccountBalancesId : IEquatable<ProvisioningAccountBalancesId>
    {
        public ProvisioningAccountBalancesId(long accountId, long assetId)
        {
            AccountId = accountId;
            AssetId = assetId;
        }

        public long AccountId { get; }
        public long AssetId { get; }

        public bool Equals(ProvisioningAccountBalancesId other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return AccountId == other.AccountId && AssetId == other.AssetId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ProvisioningAccountBalancesId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AccountId, AssetId);
        }

        public override string ToString()
        {
            return $"{AccountId}-{AssetId}";
        }
    }
}
