using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public sealed class ActiveBrokerAccountDetailsId : IEquatable<ActiveBrokerAccountDetailsId>
    {
        public ActiveBrokerAccountDetailsId(string blockchainId, long brokerAccountId)
        {
            BlockchainId = blockchainId;
            BrokerAccountId = brokerAccountId;
        }

        public string BlockchainId { get; }
        public long BrokerAccountId { get; }

        public bool Equals(ActiveBrokerAccountDetailsId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BlockchainId == other.BlockchainId && BrokerAccountId == other.BrokerAccountId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ActiveBrokerAccountDetailsId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BlockchainId, BrokerAccountId);
        }

        public override string ToString()
        {
            return $"{BlockchainId}-{BrokerAccountId}";
        }
    }
}
