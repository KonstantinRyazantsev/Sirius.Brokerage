using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Persistence.Entities.Deposits
{
    [Table(name: Tables.Deposits)]
    public class DepositEntity
    {
        public DepositEntity()
        {
            Fees = new HashSet<DepositFeeEntity>();
            Sources = new HashSet<DepositSourceEntity>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public uint Version { get; set; }
        public long Sequence { get; set; }
        public string TenantId { get; set; }
        public string BlockchainId { get; set; }
        public long BrokerAccountId { get; set; }
        public long BrokerAccountRequisitesId { get; set; }
        public long? AccountRequisitesId { get; set; }
        public long AssetId { get; set; }
        public decimal Amount { get; set; }
        public long? ConsolidationOperationId { get; set; }
        public ICollection<DepositFeeEntity> Fees { get; set; }
        public string TransactionId { get; set; }
        public long TransactionBlock { get; set; }
        public long TransactionRequiredConfirmationsCount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string ErrorMessage { get; set; }
        public DepositErrorCode? ErrorCode { get; set; }
        public DepositStateEnum State { get; set; }
        public ICollection<DepositSourceEntity> Sources { get; set; }
        public DateTimeOffset DetectedAt { get; set; }
        public DateTimeOffset? ConfirmedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset? FailedAt { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
    }
}
