using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public sealed class BrokerAccountDetailsId : IEquatable<BrokerAccountDetailsId>
    {
        public BrokerAccountDetailsId(string blockchainId, string address)
        {
            BlockchainId = blockchainId;
            Address = address;
        }

        public string BlockchainId { get; }
        public string Address { get; }

        public bool Equals(BrokerAccountDetailsId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BlockchainId == other.BlockchainId && Address == other.Address;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is BrokerAccountDetailsId other && Equals(other);
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
