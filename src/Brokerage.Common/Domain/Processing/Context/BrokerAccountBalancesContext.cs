using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountBalancesContext
    {
        public BrokerAccountBalancesContext(BrokerAccountBalances balances, 
            decimal income, 
            decimal outcome)
        {
            Balances = balances;
            Income = income;
            Outcome = outcome;
        }
        
        public BrokerAccountBalances Balances { get; }
        public decimal Income { get; }
        public decimal Outcome { get; }
    }
}
