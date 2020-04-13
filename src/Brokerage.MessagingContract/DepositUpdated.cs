using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class DepositUpdated
    {
        public long DepositId { get; set; }
        public long Sequence { get; set; }
        public long BrokerAccountRequisitesId { get; set; }
        public long? AccountRequisitesId { get; set; }
        public Unit Unit { get; set; }
        public IReadOnlyCollection<Unit> Fees { get; set; }
        public TransactionInfo TransactionInfo { get; set; }
        public DepositError Error { get; set; }
        public IReadOnlyCollection<DepositSource> Sources { get; set; }
        public DepositState State { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
