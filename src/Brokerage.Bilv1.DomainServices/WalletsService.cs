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
        private readonly IWalletRepository _walletRepository;
        private readonly BlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;

        public WalletsService(IAssetService assetService,
            IWalletRepository walletRepository,
            BlockchainApiClientProvider blockchainApiClientProvider,
            IEnrolledBalanceRepository enrolledBalanceRepository)
        {
            _assetService = assetService;
            _walletRepository = walletRepository;
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _enrolledBalanceRepository = enrolledBalanceRepository;
        }

        public async Task<Wallet> ImportWalletAsync(
            string blockchainId,
            string networkId,
            string walletAddress,
            string pubKey)
        {
            var networkAssets = _assetService.GetAssetsFor(blockchainId, networkId);

            var blockchainApiClient = _blockchainApiClientProvider.Get(blockchainId);
            await blockchainApiClient.StartBalanceObservationAsync(walletAddress);

            foreach (var networkAsset in networkAssets)
            {
                var key = new DepositWalletKey(networkAsset.AssetId, blockchainId, walletAddress);
                await _enrolledBalanceRepository.SetBalanceAsync(key, 0, 0);
            }

            var existing = await _walletRepository.GetAsync(blockchainId, networkId, walletAddress);

            if (existing != null)
            {
                return existing;
            }

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                // TODO: Normalize address
                Address = walletAddress,
                PubKey = pubKey,
                //WithdrawalCallbackOptions = request.WithdrawalCallbackOptions,
                //DepositCallbackOptions = request.DepositCallbackOptions,
                ImportedDateTime = DateTime.UtcNow,
                IsCompromised = false
            };

            await _walletRepository.AddWalletAsync(blockchainId, networkId, wallet);

            return wallet;
        }

        public async Task DeleteWalletAsync(string blockchainId, string networkId, Guid walletId)
        {
            var wallet = await _walletRepository.GetAsync(blockchainId, networkId, walletId);

            if (wallet == null)
            {
                return;
            }

            var networkAssets = _assetService.GetAssetsFor(blockchainId, networkId);
            
            var blockchainApiClient = _blockchainApiClientProvider.Get(blockchainId);
            
            foreach (var networkAsset in networkAssets)
            {
                var key = new DepositWalletKey(networkAsset.AssetId, blockchainId, wallet.Address);

                await blockchainApiClient.StopBalanceObservationAsync(key.WalletAddress);
            }

            await _walletRepository.DeleteAsync(blockchainId, networkId, walletId);
        }

        public Task<Wallet> GetWalletAsync(string blockchainId, string networkId, Guid walletId)
        {
            return _walletRepository.GetAsync(blockchainId, networkId, walletId);
        }

        public Task<IReadOnlyCollection<Wallet>> GetWalletsAsync(string blockchainId, string networkId, int skip, int take = 100)
        {
            return _walletRepository.GetManyAsync(blockchainId, networkId, skip, take);
        }

        public Task<Wallet> GetWalletByAddressAsync(string blockchainId, string networkId, string walletAddress)
        {
            return _walletRepository.GetAsync(blockchainId, networkId, walletAddress);
        }
    }
}
