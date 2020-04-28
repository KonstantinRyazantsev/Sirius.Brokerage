using System;

namespace Brokerage.Common.Domain.Accounts
{
    public sealed class AccountDetails
    {
        private AccountDetails(
            long id,
            AccountDetailsId naturalId,
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
        public AccountDetailsId NaturalId { get; }
        public long AccountId { get; }
        public long BrokerAccountId { get; }
        public DateTime CreatedAt { get; }
        
        public static AccountDetails Create(long id,
            AccountDetailsId naturalId, 
            long accountId,
            long brokerAccountId)
        {
            return new AccountDetails(
                id,
                naturalId,
                accountId,
                brokerAccountId,
                DateTime.UtcNow);
        }

        public static AccountDetails Restore(long id,
            AccountDetailsId naturalId,
            long accountId,
            long brokerAccountId,
            DateTime createdAt)
        {
            return new AccountDetails(
                id,
                naturalId,
                accountId,
                brokerAccountId,
                createdAt);
        }
    }
}
