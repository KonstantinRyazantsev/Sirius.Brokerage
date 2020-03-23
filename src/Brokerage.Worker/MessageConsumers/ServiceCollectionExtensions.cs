using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Worker.MessageConsumers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageConsumers(this IServiceCollection services)
        {
            services.AddTransient<BlockchainUpdatesConsumer>();
            services.AddTransient<ProtocolUpdatesConsumer>();

            return services;
        }
    }
}
