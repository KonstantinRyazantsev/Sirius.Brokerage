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
    {
        private readonly ConcurrentBag<Deposit> _deposits;
        
        public TransactionProcessingContext(IReadOnlyCollection<BrokerAccountContext> brokerAccounts, 
            Operation operation,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<Deposit> deposits,
            Blockchain blockchain)
        {
            BrokerAccounts = brokerAccounts;
            Operation = operation;
            TransactionInfo = transactionInfo;
            Blockchain = blockchain;

            _deposits = new ConcurrentBag<Deposit>(deposits);
            
            BrokerAccountBalances = brokerAccounts
                .SelectMany(x => x.Balances)
                .ToDictionary(x => x.Balances.NaturalId, x => x.Balances);
        }

        public IReadOnlyCollection<Deposit> Deposits => _deposits;
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
    }
}
