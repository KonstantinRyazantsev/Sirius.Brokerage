using System.Collections.Generic;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class BrokerAccountContext
    {
        public BrokerAccountContext(string tenantId,
            long brokerAccountId,
            BrokerAccount brokerAccount,
            BrokerAccountDetails activeDetails,
            IReadOnlyCollection<AccountContext> accounts,
            IReadOnlyCollection<BrokerAccountBalancesContext> balances,
            IReadOnlyCollection<BrokerAccountContextEndpoint> inputs,
            IReadOnlyCollection<BrokerAccountContextEndpoint> outputs,
            IReadOnlyDictionary<(long brokerAccountDetailsId, long assetId), decimal> income,
            IReadOnlyDictionary<long, decimal> outcome,
            IReadOnlyDictionary<long, BrokerAccountDetails> matchedBrokerAccountDetails,
            IReadOnlyDictionary<long, BrokerAccountDetails> allBrokerAccountDetails)
        {
            Accounts = accounts;
            Balances = balances;
            Inputs = inputs;
            Outputs = outputs;
            Income = income;
            Outcome = outcome;
            MatchedBrokerAccountDetails = matchedBrokerAccountDetails;
            AllBrokerAccountDetails = allBrokerAccountDetails;
            TenantId = tenantId;
            BrokerAccountId = brokerAccountId;
            BrokerAccount = brokerAccount;
            ActiveDetails = activeDetails;
        }

        public string TenantId { get; }

        public long BrokerAccountId { get; }

        public BrokerAccount BrokerAccount { get; }

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

        /// <summary>
        /// Broker account details of the given broker account which match either sources or destinations
        /// of the transaction being processed
        /// </summary>
        public IReadOnlyDictionary<long, BrokerAccountDetails> MatchedBrokerAccountDetails { get; }

        /// <summary>
        /// Broker account details of the given broker account details including <see cref="MatchedBrokerAccountDetails"/>,
        /// current <see cref="ActiveDetails"/> and details used by deposits in this transaction if any
        /// </summary>
        public IReadOnlyDictionary<long, BrokerAccountDetails> AllBrokerAccountDetails { get; }
    }
}
