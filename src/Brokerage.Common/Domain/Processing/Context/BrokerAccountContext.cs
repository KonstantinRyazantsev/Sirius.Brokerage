using System.Collections.Generic;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountContext
    {
        public BrokerAccountContext(IReadOnlyCollection<AccountContext> accounts,
            IReadOnlyCollection<BrokerAccountBalancesContext> balances,
            IReadOnlyCollection<BrokerAccountContextEndpoint> inputs,
            IReadOnlyCollection<BrokerAccountContextEndpoint> outputs,
            IReadOnlyDictionary<long, decimal> income,
            IReadOnlyDictionary<long, decimal> outcome)
        {
            Accounts = accounts;
            Balances = balances;
            Inputs = inputs;
            Outputs = outputs;
            Income = income;
            Outcome = outcome;
        }

        public IReadOnlyCollection<AccountContext> Accounts { get; }
        public IReadOnlyCollection<BrokerAccountBalancesContext> Balances { get; }
        public IReadOnlyCollection<BrokerAccountContextEndpoint> Inputs { get; }
        public IReadOnlyCollection<BrokerAccountContextEndpoint> Outputs { get; }
        public IReadOnlyDictionary<long, decimal> Income { get; }
        public IReadOnlyDictionary<long, decimal> Outcome { get; }
    }
}
