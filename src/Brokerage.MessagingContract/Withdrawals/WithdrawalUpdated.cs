using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class WithdrawalUpdated
    {
        public long WithdrawalId { get; set; }
        public uint Version { get; set; }
        public long Sequence { get; set; }
        public long BrokerAccountId { get; set; }

        public long BrokerAccountRequisitesId { get; set; }

        public long? AccountId { get; set; }

        public string ReferenceId { get; set; }

        public Unit Unit { get; set; }

        public string TenantId { get; set; }

        public IReadOnlyCollection<Unit> Fees { get; set; }

        public DestinationRequisites DestinationRequisites { get; set; }

        public WithdrawalState State { get; set; }

        public TransactionInfo TransactionInfo { get; set; }

        public WithdrawalError Error { get; set; }

        public long? OperationId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
