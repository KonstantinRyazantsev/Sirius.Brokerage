using System.Threading.Tasks;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IWalletsService
    {
        Task ImportWalletAsync(
            string blockchainId,
            string walletAddress);
    }
}
