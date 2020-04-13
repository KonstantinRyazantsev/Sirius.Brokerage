using System.Collections.Generic;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class ProcessingContext
    {
        public ProcessingContext(IReadOnlyCollection<BrokerAccountContext> brokerAccounts, 
            Operation operation,
            TransactionInfo transactionInfo)
        {
            BrokerAccounts = brokerAccounts;
            Operation = operation;
            TransactionInfo = transactionInfo;
        }

        public IReadOnlyCollection<BrokerAccountContext> BrokerAccounts { get; }
        public Operation Operation { get; }
        public TransactionInfo TransactionInfo { get; }
    }
}
