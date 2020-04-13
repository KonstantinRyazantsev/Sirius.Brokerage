using System.Collections.Generic;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Domain.Processing
{
    internal sealed class ProcessorsProvider : IProcessorsProvider
    {
        public IReadOnlyCollection<IDetectedTransactionProcessor> DetectedTransactionProcessors => _detectedTransactionProcessors;
        public IReadOnlyCollection<IConfirmedTransactionProcessor> ConfirmedTransactionProcessors => _confirmedTransactions;
        public IReadOnlyCollection<ICompletedOperationProcessor> CompletedOperationProcessors => _completedOperationProcessors;
        public IReadOnlyCollection<IFailedOperationProcessor> FailedOperationProcessors => _failedOperationProcessors;
        public IReadOnlyCollection<ICancelledBlockProcessor> CancelledBlockProcessors => _cancelledBlockProcessors;

        private readonly List<IDetectedTransactionProcessor> _detectedTransactionProcessors;
        private readonly List<IConfirmedTransactionProcessor> _confirmedTransactions;
        private readonly List<ICompletedOperationProcessor> _completedOperationProcessors;
        private readonly List<IFailedOperationProcessor> _failedOperationProcessors;
        private readonly List<ICancelledBlockProcessor> _cancelledBlockProcessors;

        public ProcessorsProvider()
        {
            _detectedTransactionProcessors = new List<IDetectedTransactionProcessor>
            {
                new DetectedDepositProcessor(),
                new DetectedBrokerDepositProcessor()
            };
            _confirmedTransactions = new List<IConfirmedTransactionProcessor>();
            _completedOperationProcessors = new List<ICompletedOperationProcessor>();
            _failedOperationProcessors = new List<IFailedOperationProcessor>();
            _cancelledBlockProcessors = new List<ICancelledBlockProcessor>();
        }
    }
}
