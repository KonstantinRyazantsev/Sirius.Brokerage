using System;
using System.Threading.Tasks;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface ITransferService
    {
        Task<string> BuildTransactionAsync(
            Guid requestId,
            string blockchainId, 
            string blockchainAssetId,
            string networkId,
            string fromAddress,
            string toAddress,
            decimal amount,
            string pubKey = null);

        Task BroadcastTransactionAsync(
            Guid requestId, 
            string networkId,
            string blockchainId,
            string signedTransaction
        );
    }
}
