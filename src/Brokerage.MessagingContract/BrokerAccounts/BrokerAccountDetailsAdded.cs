using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts
{
    public class BrokerAccountDetailsAdded
    {
        public long BrokerAccountDetailsId { get; set; }
        public long BrokerAccountId { get; set; }
        public string BlockchainId { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
