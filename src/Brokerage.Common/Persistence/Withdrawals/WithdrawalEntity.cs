using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Brokerage.Common.Domain.Withdrawals;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Withdrawals
{
    [Table(name: Tables.Withdrawals)]
    public class WithdrawalEntity
    {
        public WithdrawalEntity()
        {
            Fees = new HashSet<WithdrawalFeeEntity>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public uint Version { get; set; }
        public long Sequence { get; set; }

        public long BrokerAccountId { get; set; }

        public long BrokerAccountRequisitesId { get; set; }

        public long? AccountId { get; set; }

        public string ReferenceId { get; set; }

        public long AssetId { get; set; }

        public decimal Amount { get; set; }

        public string TenantId { get; set; }

        public IReadOnlyCollection<WithdrawalFeeEntity> Fees { get; set; }

        //Destination
        public string DestinationAddress { get; set; }

        public string DestinationTag { get; set; }

        public DestinationTagType? DestinationTagType { get; set; }

        public WithdrawalState State { get; set; }

        //Transaction
        public string TransactionId { get; set; }
        public long? TransactionBlock { get; set; }
        public long? TransactionRequiredConfirmationsCount { get; set; }
        public DateTimeOffset? TransactionDateTime { get; set; }

        //Error
        public string WithdrawalErrorMessage { get; set; }

        public WithdrawalErrorCode? WithdrawalErrorCode { get; set; }

        public long? OperationId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
