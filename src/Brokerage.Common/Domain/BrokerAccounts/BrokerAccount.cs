using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccount
    {
        private BrokerAccount(string name, string tenantId, string requestId)
        {
            Name = name;
            TenantId = tenantId;
            RequestId = requestId;
            State = BrokerAccountState.Creating;
            CreationDateTime = DateTime.UtcNow;
        }

        private BrokerAccount(
            long brokerAccountId, 
            string name,
            string tenantId, 
            DateTime creationDateTime, 
            DateTime? blockingDateTime, 
            DateTime? activationDateTime, 
            BrokerAccountState state,
            string requestId)
        {
            BrokerAccountId = brokerAccountId;
            Name = name;
            TenantId = tenantId;
            CreationDateTime = creationDateTime;
            BlockingDateTime = blockingDateTime;
            ActivationDateTime = activationDateTime;
            State = state;
            RequestId = requestId;
        }
        
        public long BrokerAccountId { get; }
        public string Name { get; }
        public string TenantId { get; }
        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public DateTime CreationDateTime { get; }
        public DateTime? BlockingDateTime { get; }
        public DateTime? ActivationDateTime { get; private set; }
        public BrokerAccountState State { get; private set; }

        public bool IsOwnedBy(string tenantId)
        {
            return TenantId == tenantId;
        }

        public static BrokerAccount Create(string name, string tenantId, string requestId)
        {
            return new BrokerAccount(name, tenantId, requestId);
        }

        public static BrokerAccount Restore(
            long brokerAccountId,
            string name,
            string tenantId,
            DateTime creationDateTime,
            DateTime? blockingDateTime,
            DateTime? activationDateTime,
            BrokerAccountState state,
            string requestId)
        {
            return new BrokerAccount(
                brokerAccountId, 
                name, 
                tenantId, 
                creationDateTime, 
                blockingDateTime, 
                activationDateTime, 
                state, 
                requestId);
        }

        public void Activate()
        {
            ActivationDateTime = DateTime.UtcNow;
            State = BrokerAccountState.Active;
        }
    }
}
