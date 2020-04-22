using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountRequisites
    {
        private BrokerAccountRequisites(
            long id,
            BrokerAccountRequisitesId naturalId,
            string tenantId,
            long brokerAccountId,
            DateTime createdAt)
        {
            Id = id;
            NaturalId = naturalId;
            BrokerAccountId = brokerAccountId;

            CreatedAt = createdAt;
            TenantId = tenantId;
        }

        public long Id { get; }
        public BrokerAccountRequisitesId NaturalId { get; }
        public string TenantId { get; }
        public long BrokerAccountId { get; }
        public DateTime CreatedAt { get; }
        
        public static BrokerAccountRequisites Create(long id,
            BrokerAccountRequisitesId naturalId,
            string tenantId,
            long brokerAccountId)
        {
            return new BrokerAccountRequisites(id, naturalId, tenantId, brokerAccountId, DateTime.UtcNow);
        }

        public static BrokerAccountRequisites Restore(long id, 
            BrokerAccountRequisitesId naturalId,
            string tenantId,
            long brokerAccountId, 
            DateTime createdAt)
        {
            return new BrokerAccountRequisites(id, naturalId, tenantId, brokerAccountId, createdAt);
        }
    }
}
