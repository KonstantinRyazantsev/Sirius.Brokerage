using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Wallets;

namespace Brokerage.Bilv1.Domain.Repositories
{
    public interface IWalletRepository
    {
        Task AddWalletAsync(string blockchainId, string networkId, Wallet wallet);
        Task<Wallet> GetAsync(string blockchainId, string networkId, string walletAddress);
        Task<Wallet> GetAsync(string blockchainId, string networkId, Guid walletId);
        Task<IReadOnlyCollection<Wallet>> GetManyAsync(string blockchainId, string networkId, int skip, int take = 100);
        Task DeleteAsync(string blockchainId, string networkId, Guid walletId);
    }
}
