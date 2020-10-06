using System.Collections.Generic;

namespace Brokerage.Common.Domain.Processing
{
    public interface IProcessorsFactory
    {
        IReadOnlyCollection<IDetectedTransactionProcessor> GetDetectedTransactionProcessors();
        IReadOnlyCollection<IConfirmedTransactionProcessor> GetConfirmedTransactionProcessors();
        IReadOnlyCollection<ISentOperationProcessor> GetSentOperationProcessors();

        IReadOnlyCollection<ISigningOperationProcessor> GetSigningOperationProcessors();
        IReadOnlyCollection<ICompletedOperationProcessor> GetCompletedOperationProcessors();
        IReadOnlyCollection<IFailedOperationProcessor> GetFailedOperationProcessors();
        IReadOnlyCollection<ICancelledBlockProcessor> GetCancelledBlockProcessors();
    }
}
