using System.Threading.Tasks;

namespace Brokerage.Bilv1.Domain.Models.Transactions.Transfers
{
    public interface ITransferCallbacksNotifier
    {
        Task HandleDetected(TransferTransaction transfer);
        Task HandleConfirmationsIncrement(TransferTransaction transfer);
        Task HandleConfirmed(TransferTransaction transfer);
        Task HandleReverted(TransferTransaction transfer);
    }
}
