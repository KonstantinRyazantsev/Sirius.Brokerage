using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Deposits.Implementations;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class TransactionProcessingContext
    {
        public static readonly TransactionProcessingContext Empty = new TransactionProcessingContext(
            Array.Empty<BrokerAccountContext>(),
            default,
            default,
            Array.Empty<Deposit>(),
            default,
            default,
            Array.Empty<Account>());

        private readonly ConcurrentBag<Deposit> _deposits;
        private readonly ConcurrentBag<Operation> _newOperations;
        private readonly ConcurrentBag<MinDepositResidual> _newMinDepositResiduals;
        private readonly ConcurrentBag<BrokerDeposit> _brokerDeposits;
        private readonly ConcurrentBag<TinyTokenDeposit> _tinyTokenDeposits;
        private readonly ConcurrentBag<TokenDeposit> _tokenDeposits;
        private readonly ConcurrentBag<RegularDeposit> _regularDeposits;
        private readonly ConcurrentBag<TinyDeposit> _tinyDeposits;

        public TransactionProcessingContext(IReadOnlyCollection<BrokerAccountContext> brokerAccounts,
            Operation operation,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<Deposit> deposits,
            Blockchain blockchain,
            IReadOnlyCollection<MinDepositResidual> minDepositResiduals,
            IReadOnlyCollection<Account> accounts)
        {
            MinDepositResiduals = minDepositResiduals;
            Accounts = accounts;
            BrokerAccounts = brokerAccounts;
            Operation = operation;
            TransactionInfo = transactionInfo;
            Blockchain = blockchain;

            var depositLookup = deposits.ToLookup(x => x.GetType());

            _deposits = new ConcurrentBag<Deposit>(deposits);
            _brokerDeposits = new ConcurrentBag<BrokerDeposit>(depositLookup[typeof(BrokerDeposit)].Cast<BrokerDeposit>());
            _regularDeposits = new ConcurrentBag<RegularDeposit>(depositLookup[typeof(RegularDeposit)].Cast<RegularDeposit>());
            _tinyDeposits = new ConcurrentBag<TinyDeposit>(depositLookup[typeof(TinyDeposit)].Cast<TinyDeposit>());
            _tinyTokenDeposits = new ConcurrentBag<TinyTokenDeposit>(depositLookup[typeof(TinyTokenDeposit)].Cast<TinyTokenDeposit>());
            _tokenDeposits = new ConcurrentBag<TokenDeposit>(depositLookup[typeof(TokenDeposit)].Cast<TokenDeposit>());
            _newOperations = new ConcurrentBag<Operation>();
            _newMinDepositResiduals = new ConcurrentBag<MinDepositResidual>();

            BrokerAccountBalances = brokerAccounts
                .SelectMany(x => x.Balances)
                .ToDictionary(x => x.Balances.NaturalId, x => x.Balances);
        }

        public IReadOnlyCollection<MinDepositResidual> MinDepositResiduals { get; private set; }
        public IReadOnlyCollection<Account> Accounts { get; }

        public IReadOnlyCollection<Operation> NewOperations => _newOperations;

        public IReadOnlyCollection<MinDepositResidual> NewMinDepositResiduals => _newMinDepositResiduals;
        public IReadOnlyCollection<BrokerAccountContext> BrokerAccounts { get; }
        public Operation Operation { get; }
        public TransactionInfo TransactionInfo { get; }
        public Blockchain Blockchain { get; }
        public IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> BrokerAccountBalances { get; }

        public bool IsEmpty => !Deposits.Any() &&
                               !BrokerAccounts.Any() &&
                               !BrokerAccountBalances.Any() &&
                               Operation == null;

        public IReadOnlyCollection<Deposit> Deposits => _deposits;
        public IReadOnlyCollection<BrokerDeposit> BrokerDeposits => _brokerDeposits;

        public IReadOnlyCollection<TinyTokenDeposit> TinyTokenDeposits => _tinyTokenDeposits;

        public IReadOnlyCollection<TokenDeposit> TokenDeposits => _tokenDeposits;

        public IReadOnlyCollection<RegularDeposit> RegularDeposits => _regularDeposits;

        public IReadOnlyCollection<TinyDeposit> TinyDeposits => _tinyDeposits;

        public void AddDeposit(Deposit deposit)
        {
            _deposits.Add(deposit);
            switch (deposit.DepositType)
            {
                case DepositType.Regular:
                    _regularDeposits.Add(deposit as RegularDeposit);
                    break;
                case DepositType.Broker:
                    _brokerDeposits.Add(deposit as BrokerDeposit);
                    break;
                case DepositType.Tiny:
                    _tinyDeposits.Add(deposit as TinyDeposit);
                    break;
                case DepositType.Token:
                    _tokenDeposits.Add(deposit as TokenDeposit);
                    break;
                case DepositType.TinyToken:
                    _tinyTokenDeposits.Add(deposit as TinyTokenDeposit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deposit.DepositType), deposit.DepositType, null);
            }
        }

        public void AddNewOperation(Operation operation)
        {
            _newOperations.Add(operation);
        }

        public void AddNewMinDepositResidual(MinDepositResidual minDepositResidual)
        {
            _newMinDepositResiduals.Add(minDepositResidual);
        }
    }
}
