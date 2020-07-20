using System;
using System.Collections.Generic;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountContext
    {
        public BrokerAccountContext(string tenantId,
            long brokerAccountId,
            BrokerAccountDetails activeDetails,
            IReadOnlyCollection<AccountContext> accounts,
            IReadOnlyCollection<BrokerAccountBalancesContext> balances,
            IReadOnlyCollection<BrokerAccountContextEndpoint> inputs,
            IReadOnlyCollection<BrokerAccountContextEndpoint> outputs,
            IReadOnlyDictionary<(long brokerAccountDetailsId, long assetId), decimal> income,
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
            ActiveDetails = activeDetails;
        }

        public string TenantId { get; }

        public long BrokerAccountId { get; }

        /// <summary>
        /// Active broker account details
        /// </summary>
        public BrokerAccountDetails ActiveDetails { get; }

        /// <summary>
        /// Broker account accounts touched by the transaction. There is an input or an output to/from
        /// the account details
        /// </summary>
        public IReadOnlyCollection<AccountContext> Accounts { get; }

        /// <summary>
        /// All broker account balances touched by the transaction. There is an input or an output to/from
        /// the account or broker account details in the given asset 
        /// </summary>
        public IReadOnlyCollection<BrokerAccountBalancesContext> Balances { get; }

        /// <summary>
        /// Inputs to all broker account details in the given tx
        /// </summary>
        public IReadOnlyCollection<BrokerAccountContextEndpoint> Inputs { get; }

        /// <summary>
        /// Outputs from all broker account details in the given tx
        /// </summary>
        public IReadOnlyCollection<BrokerAccountContextEndpoint> Outputs { get; }

        /// <summary>
        /// Income to all broker account details in the given transaction, indexed by asset ID
        /// </summary>
        public IReadOnlyDictionary<(long brokerAccountDetailsId, long assetId), decimal> Income { get; }

        /// <summary>
        /// Outcome from all broker account details in the given transaction, indexed by asset ID
        /// </summary>
        public IReadOnlyDictionary<long, decimal> Outcome { get; }
    }
}
