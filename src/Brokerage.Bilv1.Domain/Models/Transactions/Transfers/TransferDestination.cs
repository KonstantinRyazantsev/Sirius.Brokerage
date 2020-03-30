using System.Collections.Generic;

namespace Brokerage.Bilv1.Domain.Models.Transactions.Transfers
{
    public sealed class TransferDestination
    {
        public string Address { get; set; }
        public string Tag { get; set; }
        public DestinationTagType TagType { get; set; }
        public IReadOnlyCollection<TransferUnit> Units { get; set; }
    }
}
