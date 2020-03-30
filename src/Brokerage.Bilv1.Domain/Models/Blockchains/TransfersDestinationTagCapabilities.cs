using System.Collections.Generic;
using System.Numerics;

namespace Brokerage.Bilv1.Domain.Models.Blockchains
{
    public sealed class TransfersDestinationTagCapabilities
    {
        public bool Number { get; set; }
        public BigInteger MinNumber { get; set; }
        public BigInteger MaxNumber { get; set; }
        public bool Text { get; set; }
        public int MaxTextLength { get; set; }
        public IReadOnlyDictionary<string, string> NumberTagNames { get; set; }
        public IReadOnlyDictionary<string, string> TextTagNames { get; set; }
    }
}
