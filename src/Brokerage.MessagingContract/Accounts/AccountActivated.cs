using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Accounts
{
    public class AccountActivated
    {
        public long AccountId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
