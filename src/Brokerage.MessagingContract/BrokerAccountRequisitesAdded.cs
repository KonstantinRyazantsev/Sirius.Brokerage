using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class BrokerAccountRequisitesAdded
    {
        public long BrokerAccountRequisitesId { get; set; }

        public long BrokerAccountId { get; set; }

        public string BlockchainId { get; set; }

        public string Address { get; set; }

        public DateTime CreationDateTime { get; set; }
    }
}
