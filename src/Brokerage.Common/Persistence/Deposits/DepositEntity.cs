using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Persistence.Deposits
{
    [Table(name: Tables.Deposits)]
    public class DepositEntity
    {
        public DepositEntity()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public uint Version { get; set; }
        public long Sequence { get; set; }
        public string TenantId { get; set; }
        public string BlockchainId { get; set; }
        public long BrokerAccountId { get; set; }
        public long BrokerAccountDetailsId { get; set; }
        public long? AccountDetailsId { get; set; }
        public long AssetId { get; set; }
        public decimal Amount { get; set; }
        public long? ConsolidationOperationId { get; set; }
        public IReadOnlyCollection<DepositFeeEntity> Fees { get; set; }
        public string TransactionId { get; set; }
        public long TransactionBlock { get; set; }
        public long TransactionRequiredConfirmationsCount { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string ErrorMessage { get; set; }
        public DepositErrorCode? ErrorCode { get; set; }
        public DepositStateEnum State { get; set; }
        public IReadOnlyCollection<DepositSourceEntity> Sources { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public decimal? MinDepositForConsolidation { get; set; }
    }
}
