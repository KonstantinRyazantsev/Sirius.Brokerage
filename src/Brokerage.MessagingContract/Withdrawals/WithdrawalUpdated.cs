using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class WithdrawalUpdated
    {
        public long WithdrawalId { get; set; }

        public string TenantId { get; set; }

        public long BrokerAccountId { get; set; }

        public long BrokerAccountDetailsId { get; set; }

        public long? AccountId { get; set; }

        public Unit Unit { get; set; }

        public IReadOnlyCollection<Unit> Fees { get; set; }

        public DestinationDetails DestinationDetails { get; set; }

        public WithdrawalState State { get; set; }

        public TransactionInfo TransactionInfo { get; set; }

        public WithdrawalError Error { get; set; }

        public TransferContext TransferContext { get; set; }

        public long? OperationId { get; set; }

        public long Sequence { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
