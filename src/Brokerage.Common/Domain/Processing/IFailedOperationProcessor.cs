using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Common.Domain.Processing
{
    public interface IFailedOperationProcessor
    {
        Task Process(OperationFailed evt, OperationProcessingContext processingContext);
    }
}
