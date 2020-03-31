using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Assets;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;
using Brokerage.Bilv1.Domain.Models.Wallets;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.Domain.Services;

namespace Brokerage.Bilv1.DomainServices
{
    public class BalanceService : IBalanceService
    {
        private readonly IAssetService _assetService;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;

        public BalanceService(IAssetService assetService,
            IEnrolledBalanceRepository enrolledBalanceRepository)
        {
            _assetService = assetService;
            _enrolledBalanceRepository = enrolledBalanceRepository;
        }

        public async Task<IReadOnlyCollection<WalletBalancesDomain>> GetBalancesForBlockchain(string blockchainId, string networkId, int skip, int take)
        {
            var balances = await _enrolledBalanceRepository.GetAllAsync(skip, take);

            var lookup = balances
                .GroupBy(x => x.Key.WalletAddress)
                .Select(walletsGroup => new WalletBalancesDomain
                {
                    Address = walletsGroup.Key,
                    Balances = walletsGroup
                        .Select(x =>
                        {
                            var asset = _assetService.GetAssetForId(blockchainId, x.Key.BlockchainAssetId);

                            if (asset == null)
                                return null;

                            var amount = ConverterExtensions.ConvertFromString(x.Balance.ToString(), asset.Accuracy, asset.Accuracy);

                            return new WalletAssetBalanceDomain()
                            {
                                AssetId = x.Key.BlockchainAssetId,
                                Balance = amount,
                                LastUpdateBlock = x.Block
                            };
                        })
                      .Where(x => x != null)
                      .ToArray()
                })
                .ToArray();

            return lookup;
        }

        public async Task<WalletBalancesDomain> GetBalancesForWallet(string blockchainId, string networkId, string walletAddress)
        {
            var balances = new List<WalletAssetBalanceDomain>();
            var assets = _assetService.GetAssetsFor(blockchainId, networkId);

            foreach (var asset in assets)
            {
                var walletBalance = await GetWalletAssetBalance(blockchainId, walletAddress, asset);
                balances.Add(walletBalance);
            }

            return new WalletBalancesDomain()
            {
                Address = walletAddress,
                Balances = balances
            };
        }

        public async Task<WalletBalancesDomain> GetBalancesForWalletAndAsset(
            string blockchainId,
            string networkId,
            string walletAddress,
            string assetId)
        {
            var asset = _assetService.GetAssetForId(blockchainId, assetId);

            var walletBalance = await GetWalletAssetBalance(blockchainId, walletAddress, asset);

            return new WalletBalancesDomain()
            {
                Address = walletAddress,
                Balances = new[] { walletBalance }
            };
        }

        private async Task<WalletAssetBalanceDomain> GetWalletAssetBalance(string blockchainId, string walletAddress, Asset asset)
        {
            var balance = await _enrolledBalanceRepository.TryGetAsync(new DepositWalletKey(asset.AssetId, blockchainId,walletAddress));

            var amount = ConverterExtensions.ConvertFromString(balance.Balance.ToString(), asset.Accuracy, asset.Accuracy);
            var walletBalance = (new WalletAssetBalanceDomain()
            {
                AssetId = asset.AssetId,
                Balance = amount,
                LastUpdateBlock = balance.Block
            });

            return walletBalance;
        }
    }
}
