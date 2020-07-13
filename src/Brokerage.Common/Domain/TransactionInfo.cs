using System;

namespace Brokerage.Common.Domain
{
    public class TransactionInfo
    {
        public TransactionInfo(string transactionId, long transactionBlock, long requiredConfirmationsCount,
            DateTime dateTime)
        {
            TransactionId = transactionId;
            TransactionBlock = transactionBlock;
            RequiredConfirmationsCount = requiredConfirmationsCount;
            DateTime = dateTime;
        }

        public string TransactionId { get; }
        public long TransactionBlock { get; }
        public long RequiredConfirmationsCount { get; }
        public DateTime DateTime { get; }

        public TransactionInfo UpdateRequiredConfirmationsCount(long requiredConfirmationsCount)
        {
            return new TransactionInfo(
                TransactionId,
                TransactionBlock,
                requiredConfirmationsCount,
                DateTime);
        }
    }
}
