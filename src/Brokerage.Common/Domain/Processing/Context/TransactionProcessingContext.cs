using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class TransactionProcessingContext
    { public static readonly TransactionProcessingContext Empty = new TransactionProcessingContext(
            Array.Empty<BrokerAccountContext>(),
            default,
            default,
            Array.Empty<Deposit>(),
            default,
            default);

        private readonly ConcurrentBag<Deposit> _deposits;
        private readonly ConcurrentBag<Operation> _newOperations;
        private readonly ConcurrentBag<MinDepositResidual> _newMinDepositResiduals;

        public TransactionProcessingContext(IReadOnlyCollection<BrokerAccountContext> brokerAccounts, 
            Operation operation,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<Deposit> deposits,
            Blockchain blockchain,
            IReadOnlyCollection<MinDepositResidual> minDepositResiduals)
        {
            MinDepositResiduals = minDepositResiduals;
            BrokerAccounts = brokerAccounts;
            Operation = operation;
            TransactionInfo = transactionInfo;
            Blockchain = blockchain;

            _deposits = new ConcurrentBag<Deposit>(deposits);
            _newOperations = new ConcurrentBag<Operation>();
            _newMinDepositResiduals = new ConcurrentBag<MinDepositResidual>();

            BrokerAccountBalances = brokerAccounts
                .SelectMany(x => x.Balances)
                .ToDictionary(x => x.Balances.NaturalId, x => x.Balances);
        }

        public IReadOnlyCollection<MinDepositResidual> MinDepositResiduals { get; private set; }

        public IReadOnlyCollection<Deposit> Deposits => _deposits;
        public IReadOnlyCollection<Operation> NewOperations => _newOperations;

        public IReadOnlyCollection<MinDepositResidual> NewMinDepositResiduals => _newMinDepositResiduals;
        public IReadOnlyCollection<BrokerAccountContext> BrokerAccounts { get; }
        public Operation Operation { get; }
        public TransactionInfo TransactionInfo { get; }
        public Blockchain Blockchain { get; }
        public IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> BrokerAccountBalances  {  get; }

        public bool IsEmpty => !Deposits.Any() &&
                               !BrokerAccounts.Any() &&
                               !BrokerAccountBalances.Any() &&
                               Operation == null;
    
        public void AddDeposit(Deposit deposit)
        {
            _deposits.Add(deposit);
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
