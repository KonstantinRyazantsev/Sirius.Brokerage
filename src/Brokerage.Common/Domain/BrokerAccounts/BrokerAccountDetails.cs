using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountDetails
    {
        private BrokerAccountDetails(
            long id,
            BrokerAccountDetailsId naturalId,
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
        public BrokerAccountDetailsId NaturalId { get; }
        public string TenantId { get; }
        public long BrokerAccountId { get; }
        public DateTime CreatedAt { get; }
        
        public static BrokerAccountDetails Create(long id,
            BrokerAccountDetailsId naturalId,
            string tenantId,
            long brokerAccountId)
        {
            return new BrokerAccountDetails(id, naturalId, tenantId, brokerAccountId, DateTime.UtcNow);
        }

        public static BrokerAccountDetails Restore(long id, 
            BrokerAccountDetailsId naturalId,
            string tenantId,
            long brokerAccountId, 
            DateTime createdAt)
        {
            return new BrokerAccountDetails(id, naturalId, tenantId, brokerAccountId, createdAt);
        }
    }
}
