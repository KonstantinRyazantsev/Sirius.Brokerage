using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public long BrokerAccountRequisitesId { get; set; }
        public long? AccountRequisitesId { get; set; }
        public long AssetId { get; set; }
        public decimal Amount { get; set; }
        public long? OperationId { get; set; }
        public ICollection<DepositFeeEntity> Fees { get; set; }
        public string TransactionId { get; set; }
        public long TransactionBlock { get; set; }
        public long TransactionRequiredConfirmationsCount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string ErrorMessage { get; set; }

        public DepositErrorCodeEnum? ErrorCode { get; set; }
        public DepositStateEnum DepositState { get; set; }
        public ICollection<DepositSourceEntity> Sources { get; set; }
        public string Address { get; set; }
        public DateTimeOffset DetectedDateTime { get; set; }
        public DateTimeOffset? ConfirmedDateTime { get; set; }
        public DateTimeOffset? CompletedDateTime { get; set; }
        public DateTimeOffset? FailedDateTime { get; set; }
        public DateTimeOffset? CancelledDateTime { get; set; }
    }
}
