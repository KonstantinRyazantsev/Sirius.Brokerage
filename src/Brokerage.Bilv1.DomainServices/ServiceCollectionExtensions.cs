using Brokerage.Bilv1.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Bilv1.DomainServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddTransient<IAssetService, AssetService>();
            services.AddTransient<INetworkService, NetworkService>();
            services.AddTransient<ITransferService, TransferService>();
            services.AddTransient<IBalanceService, BalanceService>();
            services.AddTransient<ITransactionOrderService, TransactionOrderService>();
            services.AddTransient<IWalletsService, WalletsService>();
            services.AddTransient<IWalletObservationService, WalletObservationService>();
            services.AddTransient<IBlockchainService, BlockchainService>();

            services.AddSingleton<BlockchainApiClientProvider>();

            return services;
        }
    }
}
