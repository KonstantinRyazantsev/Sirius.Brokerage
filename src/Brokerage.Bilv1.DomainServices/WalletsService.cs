using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;
using Brokerage.Bilv1.Domain.Models.Wallets;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.Domain.Services;

namespace Brokerage.Bilv1.DomainServices
{
    public class WalletsService : IWalletsService
    {
        private readonly IAssetService _assetService;
        private readonly BlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;

        public WalletsService(IAssetService assetService,
            BlockchainApiClientProvider blockchainApiClientProvider,
            IEnrolledBalanceRepository enrolledBalanceRepository)
        {
            _assetService = assetService;
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _enrolledBalanceRepository = enrolledBalanceRepository;
        }

        public async Task ImportWalletAsync(
            string blockchainId,
            string walletAddress)
        {
            var networkAssets = _assetService.GetAssetsFor(blockchainId);

            var blockchainApiClient = _blockchainApiClientProvider.Get(blockchainId);
            await blockchainApiClient.StartBalanceObservationAsync(walletAddress);

            foreach (var networkAsset in networkAssets)
            {
                var key = new DepositWalletKey(networkAsset.AssetId, blockchainId, walletAddress);
                await _enrolledBalanceRepository.SetBalanceAsync(key, 0, 0);
            }
        }
    }
}
