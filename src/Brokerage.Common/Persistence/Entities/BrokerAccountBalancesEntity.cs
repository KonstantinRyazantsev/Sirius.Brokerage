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
        public long Id { get; set; }
        public string NaturalId { get; set; }
        public uint Version { get; set; }
        public long Sequence { get; set; }
        public long BrokerAccountId { get; set; }
        public BrokerAccountEntity BrokerAccount { get; set; }
        public long AssetId { get; set; }
        public decimal OwnedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public decimal ReservedBalance { get; set; }
        public DateTimeOffset OwnedBalanceUpdatedAt { get; set; }
        public DateTimeOffset AvailableBalanceUpdatedAt { get; set; }
        public DateTimeOffset PendingBalanceUpdatedAt { get; set; }
        public DateTimeOffset ReservedBalanceUpdateDatedAt { get; set; }
    }
}
