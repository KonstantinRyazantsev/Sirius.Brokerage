using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Worker.MessageConsumers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
        {
            services.AddTransient<BlockchainUpdatesConsumer>();
            services.AddTransient<FinalizeBrokerAccountCreationConsumer>();
            services.AddTransient<FinalizeAccountCreationConsumer>();
            services.AddTransient<PublishAccountRequisitesConsumer>();
            services.AddTransient<OperationCompletedConsumer>();
            services.AddTransient<OperationFailedConsumer>();

            return services;
        }
    }
}
