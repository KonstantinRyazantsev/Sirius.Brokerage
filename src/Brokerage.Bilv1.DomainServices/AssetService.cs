using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Assets;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.Domain.Services;
using Microsoft.Extensions.Logging;
using Polly;

namespace Brokerage.Bilv1.DomainServices
{
    public class WalletObservationService : IWalletObservationService
    {
        private readonly BlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly IAssetService _assetService;
        private readonly ILogger<WalletObservationService> _logger;

        public WalletObservationService(
            BlockchainApiClientProvider blockchainApiClientProvider,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IAssetService assetService,
            ILogger<WalletObservationService> logger)
        {
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _assetService = assetService;
            _logger = logger;
        }

        public async Task RegisterWalletAsync(DepositWalletKey key)
        {
            var client = _blockchainApiClientProvider.Get(key.BlockchainId);

            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    (i, context) => TimeSpan.FromMilliseconds(100 * (int) Math.Pow(2, i)),
                    (exception, span, arg3) =>
                    {
                        _logger.LogWarning(exception, "Failed to register wallet. Retry");

                        return Task.CompletedTask;
                    })
                .ExecuteAsync(async () =>
                {
                    await _enrolledBalanceRepository.SetBalanceAsync(key, 0, 0);
                    await client.StartBalanceObservationAsync(key.WalletAddress);
                });
        }

        public async Task DeleteWalletAsync(DepositWalletKey key)
        {
            var client = _blockchainApiClientProvider.Get(key.BlockchainId);
            //await _enrolledBalanceRepository.DeleteBalanceAsync(key, 0, 0);
            await client.StopBalanceObservationAsync(key.WalletAddress);
        }
    }

    public class AssetService : IAssetService
    {
        private readonly Dictionary<(string blockchainId, string networkId), Asset[]> _assets;

        public AssetService()
        {
            _assets = new Dictionary<(string, string), Asset[]>
            {
                [("Bitcoin", "TestNet")] = new[]
                {
                    new Asset
                    {
                        AssetId = "BTC",
                        Ticker = "BTC",
                        Accuracy = 8
                    }

                },
                [("Bitcoin", "RegTest")] = new[]
                {
                    new Asset
                    {
                        AssetId = "BTC",
                        Ticker = "BTC",
                        Accuracy = 8
                    }
                },
                [("Bitcoin", "MainNet")] = new[]
                {
                    new Asset
                    {
                        AssetId = "BTC",
                        Ticker = "BTC",
                        Accuracy = 8
                    }

                },
                [("Ethereum", "Ropsten")] = new[]
                {
                    new Asset
                    {
                        AssetId = "ETH",
                        Ticker = "ETH",
                        Accuracy = 18
                    }
                },
                [("Ethereum", "MainNet")] = new[]
                {
                    new Asset
                    {
                        AssetId = "ETH",
                        Ticker = "ETH",
                        Accuracy = 18
                    },
                    new Asset
                    {
                        AssetId = "0xdac17f958d2ee523a2206206994597c13d831ec7",
                        Ticker = "USDT",
                        Address = "0xdac17f958d2ee523a2206206994597c13d831ec7"
                    }
                }
            };
        }

        public IReadOnlyDictionary<(string blockchainId, string networkId), Asset[]> GetAllAssets()
        {
            return _assets;
        }

        public IReadOnlyCollection<Asset> GetAssetsFor(string blockchainId, string networkId)
        {
            _assets.TryGetValue((blockchainId, networkId), out var assets);

            return assets ?? new Asset[0];
        }

        public Asset GetAssetForId(string blockchainId, string networkId, string assetId)
        {
            _assets.TryGetValue((blockchainId, networkId), out var assets);

            var result = assets?.SingleOrDefault(x => x.AssetId == assetId);

            return result;
        }

        public IReadOnlyCollection<Asset> GetAssetsForTicker(string blockchainId, string networkId, string ticker)
        {
            _assets.TryGetValue((blockchainId, networkId), out var assets);

            var result = assets?.Where(x => x.Ticker == ticker).ToArray();

            return result ?? new Asset[0];
        }
    }
}
