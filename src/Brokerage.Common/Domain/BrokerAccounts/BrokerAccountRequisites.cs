using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountRequisites
    {
        private BrokerAccountRequisites(
            long id,
            BrokerAccountRequisitesId naturalId,
            long brokerAccountId,
            DateTime createdAt)
        {
            Id = id;
            NaturalId = naturalId;
            BrokerAccountId = brokerAccountId;

            CreatedAt = createdAt;
        }

        public long Id { get; }
        public BrokerAccountRequisitesId NaturalId { get; }
        public long BrokerAccountId { get; }
        public DateTime CreatedAt { get; }
        
        public static BrokerAccountRequisites Create(long id,
            BrokerAccountRequisitesId naturalId,
            long brokerAccountId)
        {
            return new BrokerAccountRequisites(id, naturalId, brokerAccountId, DateTime.UtcNow);
        }

        public static BrokerAccountRequisites Restore(long id, 
            BrokerAccountRequisitesId naturalId,
            long brokerAccountId, 
            DateTime createdAt)
        {
            return new BrokerAccountRequisites(id, naturalId, brokerAccountId, createdAt);
        }
    }
}
