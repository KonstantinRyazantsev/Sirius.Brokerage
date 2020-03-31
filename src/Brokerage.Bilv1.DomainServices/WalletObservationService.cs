using System;
using System.Threading.Tasks;
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
}
