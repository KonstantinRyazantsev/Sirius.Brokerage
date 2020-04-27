using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class BrokerAccountBalancesUpdated
    {
        public long BrokerAccountBalancesId { get; set; }
        public long Sequence { get; set; }
        public long BrokerAccountId { get; set; }
        public long AssetId { get; set; }
        public decimal OwnedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal ReservedBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
