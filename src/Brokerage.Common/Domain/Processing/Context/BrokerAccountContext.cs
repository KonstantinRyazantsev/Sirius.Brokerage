using System.Collections.Generic;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountContext
    {
        public BrokerAccountContext(string tenantId,
            long brokerAccountId,
            BrokerAccountRequisites activeRequisites,
            IReadOnlyCollection<AccountContext> accounts,
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
            TenantId = tenantId;
            BrokerAccountId = brokerAccountId;
            ActiveRequisites = activeRequisites;
        }

        public string TenantId { get; }

        public long BrokerAccountId { get; }

        /// <summary>
        /// Active broker account requisites
        /// </summary>
        public BrokerAccountRequisites ActiveRequisites { get; }

        /// <summary>
        /// Broker account accounts touched by the transaction. There is an input or an output to/from
        /// the account requisites
        /// </summary>
        public IReadOnlyCollection<AccountContext> Accounts { get; }

        /// <summary>
        /// All broker account balances touched by the transaction. There is an input or an output to/from
        /// the account or broker account requisites in the given asset 
        /// </summary>
        public IReadOnlyCollection<BrokerAccountBalancesContext> Balances { get; }
        
        /// <summary>
        /// Inputs to all broker account requisites in the given tx
        /// </summary>
        public IReadOnlyCollection<BrokerAccountContextEndpoint> Inputs { get; }
        
        /// <summary>
        /// Outputs from all broker account requisites in the given tx
        /// </summary>
        public IReadOnlyCollection<BrokerAccountContextEndpoint> Outputs { get; }

        /// <summary>
        /// Income to all broker account requisites in the given transaction, indexed by asset ID
        /// </summary>
        public IReadOnlyDictionary<long, decimal> Income { get; }
        
        /// <summary>
        /// Outcome from all broker account requisites in the given transaction, indexed by asset ID
        /// </summary>
        public IReadOnlyDictionary<long, decimal> Outcome { get; }
    }
}
