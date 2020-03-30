using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountBalances
    {
        public long  Id { get; set; }
        public long  Version { get; set; }
        public long  BrokerAccountId { get; set; }
        public long  AssetId { get; set; }
        public string  OwnedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal  ReservedBalance { get; set; }
        public DateTime OwnedBalanceUpdateDateTime { get; set; }
        public DateTime  AvailableBalanceUpdateDateTime { get; set; }
        public DateTime PendingBalanceUpdateDateTime { get; set; }
        public DateTime ReservedBalanceUpdateDateTime { get; set; }
    }
}
