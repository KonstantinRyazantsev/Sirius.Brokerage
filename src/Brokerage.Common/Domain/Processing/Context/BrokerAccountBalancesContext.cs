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
        
        /// <summary>
        /// Income to all broker account details in the given transaction and asset. Asset matches the balances asset
        /// </summary>
        public decimal Income { get; }

        /// <summary>
        /// Income from all broker account details in the given transaction and asset. Asset matches the balances asset
        /// </summary>
        public decimal Outcome { get; }
    }
}
