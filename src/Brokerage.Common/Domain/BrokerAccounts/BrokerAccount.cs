using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccount
    {
        private BrokerAccount(string name, string tenantId)
        {
            Name = name;
            TenantId = tenantId;
            State = BrokerAccountState.Creating;
        }

        private BrokerAccount(
            long brokerAccountId, 
            string name,
            string tenantId, 
            DateTime creationDateTime, 
            DateTime? blockingDateTime, 
            DateTime? activationDateTime, 
            BrokerAccountState state)
        {
            BrokerAccountId = brokerAccountId;
            Name = name;
            TenantId = tenantId;
            CreationDateTime = creationDateTime;
            BlockingDateTime = blockingDateTime;
            ActivationDateTime = activationDateTime;
            State = state;
        }
        public long BrokerAccountId { get; }

        public string Name { get; }

        public string TenantId { get; }

        public DateTime CreationDateTime { get; }

        public DateTime? BlockingDateTime { get; }

        public DateTime? ActivationDateTime { get; }

        public BrokerAccountState State { get; }

        public bool IsOwnedBy(string tenantId)
        {
            return this.TenantId == tenantId;
        }

        public static BrokerAccount Create(string name, string tenantId)
        {
            return new BrokerAccount(name, tenantId);
        }

        public static BrokerAccount Restore(
            long brokerAccountId,
            string name,
            string tenantId,
            DateTime creationDateTime,
            DateTime? blockingDateTime,
            DateTime? activationDateTime,
            BrokerAccountState state)
        {
            return new BrokerAccount(brokerAccountId, name, tenantId, creationDateTime, blockingDateTime, activationDateTime, state);
        }
    }
}
