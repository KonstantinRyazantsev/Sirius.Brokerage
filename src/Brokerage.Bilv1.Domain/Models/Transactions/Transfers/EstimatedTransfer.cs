using System.Collections.Generic;

namespace Brokerage.Bilv1.Domain.Models.Transactions.Transfers
{
    public sealed class EstimatedTransfer
    {
        public IReadOnlyCollection<FeeEstimation> Fees { get; set; }
    }
}
