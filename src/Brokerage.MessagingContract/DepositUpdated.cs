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
        public IReadOnlyCollection<DepositFee> Fees { get; set; }
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

    public class DepositTransactionInfo
    {
        public string TransactionId { get; set; }
        public long TransactionBlock { get; set; }
        public long RequiredConfirmationsCount { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class DepositFee
    {
        public long AssetId { get; set; }
        public decimal Amount { get; set; }
    }

    public enum DepositState
    {
        Detected,
        Confirmed,
        Completed,
        Failed,
        Cancelled
    }

    public class DepositError
    {
        public string Message { get; set; }

        public DepositErrorCode Code { get; set; }

        public enum DepositErrorCode
        {
            TechnicalProblem
        }
    }

    public class DepositSource
    {
        public string Address { get; set; }

        public decimal Amount { get; set; }
    }
}
