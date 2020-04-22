
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class SourceContext
    {
        public SourceContext(string address, Unit unit)
        {
            Address = address;
            Unit = unit;
        }

        public string Address { get; }
        public Unit Unit { get; }
    }
}
