using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class OperationProcessingContext
    {
        public static readonly OperationProcessingContext Empty = new OperationProcessingContext(
            null, 
            Array.Empty<Deposit>(),
            Array.Empty<Withdrawal>(),
            new Dictionary<BrokerAccountBalancesId, BrokerAccountBalances>());

        public OperationProcessingContext(Operation operation, 
            IReadOnlyCollection<Deposit> deposits, 
            IReadOnlyCollection<Withdrawal> withdrawals,
            IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> brokerAccountBalances)
        {
            Operation = operation;
            Deposits = deposits;
            Withdrawals = withdrawals;
            BrokerAccountBalances = brokerAccountBalances;
        }

        public Operation Operation { get; }
        public IReadOnlyCollection<Deposit> Deposits { get; }
        public IReadOnlyCollection<Withdrawal> Withdrawals { get; }
        public IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> BrokerAccountBalances { get; }

        public bool IsEmpty => Operation == null &&
                               !Deposits.Any() &&
                               !Withdrawals.Any() &&
                               !BrokerAccountBalances.Any();
    }
}
