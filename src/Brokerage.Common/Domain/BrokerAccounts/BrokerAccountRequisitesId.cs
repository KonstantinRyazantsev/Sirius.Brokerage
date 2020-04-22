using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public sealed class BrokerAccountRequisitesId : IEquatable<BrokerAccountRequisitesId>
    {
        public BrokerAccountRequisitesId(string blockchainId, string address)
        {
            BlockchainId = blockchainId;
            Address = address;
        }

        public string BlockchainId { get; }
        public string Address { get; }

        public bool Equals(BrokerAccountRequisitesId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BlockchainId == other.BlockchainId && Address == other.Address;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is BrokerAccountRequisitesId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BlockchainId, Address);
        }

        public override string ToString()
        {
            return $"{BlockchainId}-{Address}";
        }
    }
}
