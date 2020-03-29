using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class AccountActivated
    {
        public long AccountId { get; set; }
        public DateTime ActivationDate { get; set; }
    }
}
