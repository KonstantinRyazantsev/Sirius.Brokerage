using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccount
    {
        private BrokerAccount(
            long id, 
            string name,
            string tenantId, 
            DateTime createdAt, 
            DateTime updatedAt, 
            BrokerAccountState state,
            string requestId,
            long vaultId)
        {
            Id = id;
            Name = name;
            TenantId = tenantId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            State = state;
            RequestId = requestId;
            VaultId = vaultId;
        }
        
        public long Id { get; }
        public string Name { get; }
        public string TenantId { get; }
        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public BrokerAccountState State { get; private set; }

        public long VaultId { get; }

        public bool IsOwnedBy(string tenantId)
        {
            return TenantId == tenantId;
        }

        public static BrokerAccount Create(string name, string tenantId, string requestId, long vaultId)
        {
            var utcNow = DateTime.UtcNow;
            return new BrokerAccount(
                default,
                name, 
                tenantId,
                utcNow, 
                utcNow, 
                BrokerAccountState.Creating, 
                requestId, 
                vaultId);
        }

        public static BrokerAccount Restore(
            long id,
            string name,
            string tenantId,
            DateTime createdAt,
            DateTime updatedAt,
            BrokerAccountState state,
            string requestId,
            long vaultId)
        {
            return new BrokerAccount(
                id, 
                name, 
                tenantId, 
                createdAt, 
                updatedAt, 
                state, 
                requestId,
                vaultId);
        }

        public void Activate()
        {
            UpdatedAt = DateTime.UtcNow;
            State = BrokerAccountState.Active;
        }
    }
}
