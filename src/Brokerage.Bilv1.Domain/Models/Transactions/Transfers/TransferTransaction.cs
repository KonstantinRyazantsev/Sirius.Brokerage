using System;
using System.Collections.Generic;

namespace Brokerage.Bilv1.Domain.Models.Transactions.Transfers
{
    public sealed class TransferTransaction
    {
        // TODO: Define in future versions
        // TODO: Should be sequential guid
        public int Id { get; set; }
        public string Hash { get; set; }
        public long Block { get; set; }
        public long ConfirmationsCount { get; set; }
        public long? ConfirmedAtBlock { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsIrreversible { get; set; }
        public bool IsFailed { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? ConfirmedDateTime { get; set; }
        public IReadOnlyCollection<TransferSource> Sources { get; set; }
        public IReadOnlyCollection<TransferDestination> Destinations { get; set; }
        public IReadOnlyCollection<PaidFee> PaidFees { get; set; }
    }
}
