﻿using System;

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
            CreatedAt = DateTime.UtcNow;
        }

        private BrokerAccount(
            long brokerAccountId, 
            string name,
            string tenantId, 
            DateTime createdAt, 
            DateTime updatedAt, 
            BrokerAccountState state,
            string requestId)
        {
            BrokerAccountId = brokerAccountId;
            Name = name;
            TenantId = tenantId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            State = state;
            RequestId = requestId;
        }
        
        public long BrokerAccountId { get; }
        public string Name { get; }
        public string TenantId { get; }
        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
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
            DateTime createdAt,
            DateTime updatedAt,
            BrokerAccountState state,
            string requestId)
        {
            return new BrokerAccount(
                brokerAccountId, 
                name, 
                tenantId, 
                createdAt, 
                updatedAt, 
                state, 
                requestId);
        }

        public void Activate()
        {
            UpdatedAt = DateTime.UtcNow;
            State = BrokerAccountState.Active;
        }
    }
}
