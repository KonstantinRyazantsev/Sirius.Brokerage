using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Wallets;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IBalanceService
    {
        Task<IReadOnlyCollection<WalletBalancesDomain>>
            GetBalancesForBlockchain(string blockchainId, string networkId, int skip, int take);

        Task<WalletBalancesDomain> GetBalancesForWallet(string blockchainId, string networkId, string walletAddress);

        Task<WalletBalancesDomain> GetBalancesForWalletAndAsset(
            string blockchainId,
            string networkId,
            string walletAddress,
            string assetId);
    }
}
