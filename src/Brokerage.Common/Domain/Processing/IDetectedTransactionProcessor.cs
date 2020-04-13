using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Common.Domain.Processing
{
    public interface IDetectedTransactionProcessor
    {
        Task Process(TransactionDetected tx, ProcessingContext processingContext);
    }
}
