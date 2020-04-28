using Brokerage.Common.Domain.BrokerAccounts;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountContextEndpoint
    {
        public BrokerAccountContextEndpoint(BrokerAccountDetails details, Unit unit)
        {
            Details = details;
            Unit = unit;
        }

        public BrokerAccountDetails Details { get; }
        public Unit Unit { get; }
    }
}
