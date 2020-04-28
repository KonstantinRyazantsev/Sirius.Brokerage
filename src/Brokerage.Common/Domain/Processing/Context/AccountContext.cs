using System.Collections.Generic;
using Brokerage.Common.Domain.Accounts;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class AccountContext
    {
        public AccountContext(AccountDetails details,
            IReadOnlyCollection<Unit> inputs, 
            IReadOnlyCollection<Unit> outputs,
            IReadOnlyDictionary<long, decimal> income,
            IReadOnlyDictionary<long, decimal> outcome)
        {
            Details = details;
            Inputs = inputs;
            Outputs = outputs;
            Income = income;
            Outcome = outcome;
        }

        public AccountDetails Details { get; }

        /// <summary>
        /// Inputs to the particular account in the given tx
        /// </summary>
        public IReadOnlyCollection<Unit> Inputs { get; }

        /// <summary>
        /// Outputs from the particular account in the given tx
        /// </summary>
        public IReadOnlyCollection<Unit> Outputs { get; }

        /// <summary>
        /// Income to the particular account in the given tx, indexed by asset ID
        /// </summary>
        public IReadOnlyDictionary<long, decimal> Income { get; }

        /// <summary>
        /// Outcome to from the particular account in the given tx, indexed by asset ID
        /// </summary>
        public IReadOnlyDictionary<long, decimal> Outcome { get; }
    }
}
