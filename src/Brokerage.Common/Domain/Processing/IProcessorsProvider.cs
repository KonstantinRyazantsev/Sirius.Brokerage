using System.Collections.Generic;

namespace Brokerage.Common.Domain.Processing
{
    public interface IProcessorsProvider
    {
        IReadOnlyCollection<IDetectedTransactionProcessor> DetectedTransactionProcessors { get; }
        IReadOnlyCollection<IConfirmedTransactionProcessor> ConfirmedTransactionProcessors { get; }
        IReadOnlyCollection<ICompletedOperationProcessor> CompletedOperationProcessors { get; }
        IReadOnlyCollection<IFailedOperationProcessor> FailedOperationProcessors { get; }
        IReadOnlyCollection<ICancelledBlockProcessor> CancelledBlockProcessors { get; }
    }
}
