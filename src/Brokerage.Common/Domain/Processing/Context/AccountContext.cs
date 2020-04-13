using System.Collections.Generic;
using Brokerage.Common.Domain.Accounts;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class AccountContext
    {
        public AccountContext(AccountRequisites requisites,
            IReadOnlyCollection<Unit> inputs, 
            IReadOnlyCollection<Unit> outputs,
            IReadOnlyDictionary<long, decimal> income,
            IReadOnlyDictionary<long, decimal> outcome)
        {
            Requisites = requisites;
            Inputs = inputs;
            Outputs = outputs;
            Income = income;
            Outcome = outcome;
        }

        public AccountRequisites Requisites { get; }
        public IReadOnlyCollection<Unit> Inputs { get; }
        public IReadOnlyCollection<Unit> Outputs { get; }
        public IReadOnlyDictionary<long, decimal> Income { get; }
        public IReadOnlyDictionary<long, decimal> Outcome { get; }
    }
}
