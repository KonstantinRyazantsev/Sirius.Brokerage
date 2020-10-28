using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Deposits.Implementations;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.ReadModels.Blockchains;

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
            null);

        public OperationProcessingContext(Operation operation,
            IReadOnlyCollection<Deposit> deposits,
            IReadOnlyCollection<Withdrawal> withdrawals,
            IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> brokerAccountBalances,
            IReadOnlyCollection<MinDepositResidual> minDepositResiduals,
            Blockchain blockchain)
        {
            Operation = operation;
            Deposits = deposits;
            Withdrawals = withdrawals;
            BrokerAccountBalances = brokerAccountBalances;
            MinDepositResiduals = minDepositResiduals;
            Blockchain = blockchain;
            var depositLookup = deposits.ToLookup(x => x.GetType());
            BrokerDeposits = depositLookup[typeof(BrokerDeposit)].Cast<BrokerDeposit>().ToArray();
            RegularDeposits = depositLookup[typeof(RegularDeposit)].Cast<RegularDeposit>().ToArray();
            TinyDeposits = depositLookup[typeof(TinyDeposit)].Cast<TinyDeposit>().ToArray();
            TinyTokenDeposits = depositLookup[typeof(TinyTokenDeposit)].Cast<TinyTokenDeposit>().ToArray();
            TokenDeposits = depositLookup[typeof(TokenDeposit)].Cast<TokenDeposit>().ToArray();
        }

        public Operation Operation { get; }
        public IReadOnlyCollection<Withdrawal> Withdrawals { get; }
        public IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> BrokerAccountBalances { get; }
        public IReadOnlyCollection<MinDepositResidual> MinDepositResiduals { get; }
        public Blockchain Blockchain { get; }

        public bool IsEmpty => Operation == null &&
                               !RegularDeposits.Any() &&
                               !Withdrawals.Any() &&
                               !BrokerAccountBalances.Any();

        public IReadOnlyCollection<Deposit> Deposits { get; }
        public IReadOnlyCollection<BrokerDeposit> BrokerDeposits { get; }

        public IReadOnlyCollection<TinyTokenDeposit> TinyTokenDeposits { get; }

        public IReadOnlyCollection<TokenDeposit> TokenDeposits { get; }

        public IReadOnlyCollection<RegularDeposit> RegularDeposits { get; }

        public IReadOnlyCollection<TinyDeposit> TinyDeposits { get; }

    }
}
