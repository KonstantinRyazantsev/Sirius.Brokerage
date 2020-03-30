using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Wallets;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IWalletsService
    {
        Task<Wallet> ImportWalletAsync(
            string blockchainId, 
            string networkId, 
            string walletAddress,
            string pubKey);

        Task DeleteWalletAsync(string blockchainId, string networkId, Guid walletId);

        Task<Wallet> GetWalletAsync(string blockchainId, string networkId, Guid walletId);

        Task<IReadOnlyCollection<Wallet>> GetWalletsAsync(string blockchainId, string networkId, int skip,
            int take = 100);

        Task<Wallet> GetWalletByAddressAsync(string blockchainId, string networkId, string walletAddress);
    }
}
