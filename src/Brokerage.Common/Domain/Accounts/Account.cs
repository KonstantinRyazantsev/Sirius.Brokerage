using System;

namespace Brokerage.Common.Domain.Accounts
{
    public class Account
    {
        private Account(
            string requestId,
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState state,
            DateTime creationDateTime,
            DateTime? blockingDateTime,
            DateTime? activationDateTime)
        {
            RequestId = requestId;
            AccountId = accountId;
            BrokerAccountId = brokerAccountId;
            ReferenceId = referenceId;
            State = state;
            CreationDateTime = creationDateTime;
            BlockingDateTime = blockingDateTime;
            ActivationDateTime = activationDateTime;
        }

        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public long AccountId { get; }
        public long BrokerAccountId { get; }
        public string ReferenceId { get; }
        public AccountState State { get; private set; }
        public DateTime CreationDateTime { get; }
        public DateTime? BlockingDateTime { get; }
        public DateTime? ActivationDateTime { get; private set; }
        
        public static Account Create(
            string requestId,
            long brokerAccountId,
            string referenceId)
        {
            return new Account(
                requestId,
                default,
                brokerAccountId,
                referenceId,
                AccountState.Creating,
                DateTime.UtcNow,
                null,
                null);
        }

        public static Account Restore(
            string requestId,
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState accountState,
            DateTime creationDateTime,
            DateTime? blockingDateTime,
            DateTime? activationDateTime)
        {
            return new Account(
                requestId,
                accountId,
                brokerAccountId,
                referenceId,
                accountState,
                creationDateTime,
                blockingDateTime,
                activationDateTime);
        }

        public void Activate()
        {
            State = AccountState.Active;
            ActivationDateTime = DateTime.UtcNow;
        }
    }
}
