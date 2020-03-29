using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class BrokerAccountActivated
    {
        public long BrokerAccountId { get; set; }
        public DateTime ActivationDate { get; set; }
    }
}
