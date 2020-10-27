using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Deposits.Implementations;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class OperationProcessingContext
    {
        public static readonly OperationProcessingContext Empty = new OperationProcessingContext(
            null, 
            Array.Empty<RegularDeposit>(),
            Array.Empty<Withdrawal>(),
            new Dictionary<BrokerAccountBalancesId, BrokerAccountBalances>(),
            Array.Empty<MinDepositResidual>(),
            Array.Empty<TinyDeposit>());

        public OperationProcessingContext(Operation operation, 
            IReadOnlyCollection<RegularDeposit> regularDeposits, 
            IReadOnlyCollection<Withdrawal> withdrawals,
            IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> brokerAccountBalances,
            IReadOnlyCollection<MinDepositResidual> minDepositResiduals,
            IReadOnlyCollection<TinyDeposit> tinyDeposits)
        {
            Operation = operation;
            RegularDeposits = regularDeposits;
            Withdrawals = withdrawals;
            BrokerAccountBalances = brokerAccountBalances;
            MinDepositResiduals = minDepositResiduals;
            TinyDeposits = tinyDeposits;
        }

        public Operation Operation { get; }
        public IReadOnlyCollection<RegularDeposit> RegularDeposits { get; }
        public IReadOnlyCollection<Withdrawal> Withdrawals { get; }
        public IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> BrokerAccountBalances { get; }
        public IReadOnlyCollection<MinDepositResidual> MinDepositResiduals { get; }
        public IReadOnlyCollection<TinyDeposit> TinyDeposits { get; }

        public bool IsEmpty => Operation == null &&
                               !RegularDeposits.Any() &&
                               !Withdrawals.Any() &&
                               !BrokerAccountBalances.Any();
    }
}
