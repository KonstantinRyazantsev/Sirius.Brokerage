using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Worker.MessageConsumers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
        {
            services.AddTransient<BlockchainUpdatedConsumer>();
            services.AddTransient<FinalizeBrokerAccountCreationConsumer>();
            services.AddTransient<FinalizeAccountCreationConsumer>();
            services.AddTransient<PublishAccountDetailsConsumer>();
            services.AddTransient<TransactionDetectedConsumer>();
            services.AddTransient<TransactionConfirmedConsumer>();
            services.AddTransient<OperationSentConsumer>();
            services.AddTransient<OperationCompletedConsumer>();
            services.AddTransient<OperationFailedConsumer>();
            services.AddTransient<AssetAddedConsumer>();
            services.AddTransient<ExecuteWithdrawalConsumer>();
            services.AddTransient<WalletAddedConsumer>();

            return services;
        }
    }
}
