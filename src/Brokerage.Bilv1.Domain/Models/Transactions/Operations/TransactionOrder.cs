using System;
using System.Collections.Generic;
using Brokerage.Bilv1.Domain.Models.Transactions.Transfers;

namespace Brokerage.Bilv1.Domain.Models.Transactions.Operations
{
    public sealed class TransactionOrder
    {
        // TODO: Should be sequential guid
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? BroadcastedDateTime { get; set; }
        public DateTime? MinedDateTime { get; set; }
        public DateTime? ConfirmedDateTime { get; set; }
        public OperationState State { get; set; }
        public OperationErrorType? ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public long? TransactionBlock { get; set; }
        public long? ConfirmedAtBlock { get; set; }
        public string SignedTransaction { get; set; }
        public IReadOnlyCollection<TransferSource> Sources { get; set; }
        public IReadOnlyCollection<TransferDestination> Destinations { get; set; }
        public Expiration Expiration { get; set; }
        public IReadOnlyCollection<PaidFee> PaidFees { get; set; }
    }
}
