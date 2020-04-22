using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Processing
{
    public interface IConfirmedTransactionProcessor
    {
        Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext);
    }
}
