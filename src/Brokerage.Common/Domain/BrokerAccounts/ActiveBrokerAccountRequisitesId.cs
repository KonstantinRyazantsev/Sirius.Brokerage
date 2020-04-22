using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public sealed class ActiveBrokerAccountRequisitesId : IEquatable<ActiveBrokerAccountRequisitesId>
    {
        public ActiveBrokerAccountRequisitesId(string blockchainId, long brokerAccountId)
        {
            BlockchainId = blockchainId;
            BrokerAccountId = brokerAccountId;
        }

        public string BlockchainId { get; }
        public long BrokerAccountId { get; }

        public bool Equals(ActiveBrokerAccountRequisitesId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BlockchainId == other.BlockchainId && BrokerAccountId == other.BrokerAccountId;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ActiveBrokerAccountRequisitesId other && Equals(other);
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
