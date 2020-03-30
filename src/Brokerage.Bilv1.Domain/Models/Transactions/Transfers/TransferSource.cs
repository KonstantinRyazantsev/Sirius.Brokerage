using System.Collections.Generic;

namespace Brokerage.Bilv1.Domain.Models.Transactions.Transfers
{
    public sealed class TransferSource
    {
        public string Address { get; set; }
        public long? Nonce { get; set; }
        public IReadOnlyCollection<TransferUnit> Units { get; set; }
    }
}
