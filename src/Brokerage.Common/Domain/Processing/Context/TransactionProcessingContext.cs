using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class TransactionProcessingContext
    {
        private readonly ConcurrentBag<Deposit> _deposits;
        
        public TransactionProcessingContext(IReadOnlyCollection<BrokerAccountContext> brokerAccounts, 
            Operation operation,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<Deposit> deposits)
        {
            BrokerAccounts = brokerAccounts;
            Operation = operation;
            TransactionInfo = transactionInfo;

            _deposits = new ConcurrentBag<Deposit>(deposits);
            
            BrokerAccountBalances = brokerAccounts
                .SelectMany(x => x.Balances)
                .ToDictionary(x => x.Balances.NaturalId, x => x.Balances);
        }

        public IReadOnlyCollection<Deposit> Deposits => _deposits;
        public IReadOnlyCollection<BrokerAccountContext> BrokerAccounts { get; }
        public Operation Operation { get; }
        public TransactionInfo TransactionInfo { get; }
        public IReadOnlyDictionary<BrokerAccountBalancesId, BrokerAccountBalances> BrokerAccountBalances  {  get; }

        public void AddDeposit(Deposit deposit)
        {
            _deposits.Add(deposit);
        }
    }
}
