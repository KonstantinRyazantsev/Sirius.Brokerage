using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Accounts
{
    public class AccountAdded
    {
        public long AccountId { get; set; }

        public long BrokerAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
