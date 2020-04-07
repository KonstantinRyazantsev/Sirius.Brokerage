using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class TransactionInfo
    {
        public string TransactionId { get; set; }
        public long TransactionBlock { get; set; }
        public long RequiredConfirmationsCount { get; set; }
        public DateTime DateTime { get; set; }
    }
}
