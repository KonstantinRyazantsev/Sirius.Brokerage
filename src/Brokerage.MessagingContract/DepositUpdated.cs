using System;
using System.Collections.Generic;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class DepositUpdated
    {
        public long DepositId { get; set; }
        public long Sequence { get; set; }
        public long BrokerAccountRequisitesId { get; set; }
        public long? AccountRequisitesId { get; set; }
        public long AssetId { get; set; }
        public decimal Amount { get; set; }
        public IReadOnlyCollection<Fee> Fees { get; set; }
        public DepositTransactionInfo TransactionInfo { get; set; }
        public DepositError Error { get; set; }
        public IReadOnlyCollection<DepositSource> Sources { get; set; }

        public DepositState State { get; set; }
        public DateTime DetectedDateTime { get; set; }
        public DateTime? ConfirmedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? FailedDateTime { get; set; }
        public DateTime? CancelledDateTime { get; set; }
    }
}
