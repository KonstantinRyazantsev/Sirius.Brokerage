using System;

namespace Brokerage.Common.Domain.Accounts
{
    public sealed class AccountRequisites
    {
        private AccountRequisites(
            long id,
            AccountRequisitesId naturalId,
            long accountId,
            long brokerAccountId,
            DateTime createdAt)
        {
            Id = id;
            AccountId = accountId;
            BrokerAccountId = brokerAccountId;
            NaturalId = naturalId;
            CreatedAt = createdAt;
        }

        public long Id { get; }
        public AccountRequisitesId NaturalId { get; }
        public long AccountId { get; }
        public long BrokerAccountId { get; }
        public DateTime CreatedAt { get; }
        
        public static AccountRequisites Create(long id,
            AccountRequisitesId naturalId, 
            long accountId,
            long brokerAccountId)
        {
            return new AccountRequisites(
                id,
                naturalId,
                accountId,
                brokerAccountId,
                DateTime.UtcNow);
        }

        public static AccountRequisites Restore(long id,
            AccountRequisitesId naturalId,
            long accountId,
            long brokerAccountId,
            DateTime createdAt)
        {
            return new AccountRequisites(
                id,
                naturalId,
                accountId,
                brokerAccountId,
                createdAt);
        }
    }
}
