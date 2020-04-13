using Brokerage.Common.Domain.BrokerAccounts;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountContextEndpoint
    {
        public BrokerAccountContextEndpoint(BrokerAccountRequisites requisites, Unit unit)
        {
            Requisites = requisites;
            Unit = unit;
        }

        public BrokerAccountRequisites Requisites { get; }
        public Unit Unit { get; }
    }
}
