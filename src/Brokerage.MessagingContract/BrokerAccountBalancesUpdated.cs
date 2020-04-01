using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class BrokerAccountBalancesUpdated
    {
        public long BrokerAccountBalancesId { get; set; }
        public uint Sequence { get; set; }
        public long BrokerAccountId { get; set; }
        public long AssetId { get; set; }
        public decimal OwnedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal ReservedBalance { get; set; }
        public DateTime OwnedBalanceUpdateDateTime { get; set; }
        public DateTime AvailableBalanceUpdateDateTime { get; set; }
        public DateTime PendingBalanceUpdateDateTime { get; set; }
        public DateTime ReservedBalanceUpdateDateTime { get; set; }
    }
}
