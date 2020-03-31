using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.BrokerAccountBalances)]
    public class BrokerAccountBalancesEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BrokerAccountBalancesId { get; set; }
        public long Version { get; set; }
        public long BrokerAccountId { get; set; }

        public BrokerAccountEntity BrokerAccount { get; set; }
        public long AssetId { get; set; }
        public decimal OwnedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal ReservedBalance { get; set; }
        public DateTimeOffset OwnedBalanceUpdateDateTime { get; set; }
        public DateTimeOffset AvailableBalanceUpdateDateTime { get; set; }
        public DateTimeOffset PendingBalanceUpdateDateTime { get; set; }
        public DateTimeOffset ReservedBalanceUpdateDateTime { get; set; }
    }
}
