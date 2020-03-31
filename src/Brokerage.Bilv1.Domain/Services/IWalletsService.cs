using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Wallets;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IWalletsService
    {
        Task ImportWalletAsync(
            string blockchainId,
            string walletAddress);
    }
}
