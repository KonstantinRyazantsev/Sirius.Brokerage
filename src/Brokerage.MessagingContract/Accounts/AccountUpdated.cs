using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Accounts
{
    public class AccountUpdated
    {
        public long AccountId { get; set; }
        public long BrokerAccountId { get; set; }
        public string ReferenceId { get; set; }
        public AccountState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Sequence { get; set; }
    }
}
