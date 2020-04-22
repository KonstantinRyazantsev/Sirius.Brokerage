using Brokerage.Common.Domain.Deposits.Processors;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IProcessorsFactory, ProcessorsFactory>();
            services.AddTransient<IOperationsExecutor, OperationsExecutor>();

            services.AddTransient<DetectedDepositProcessor>();
            services.AddTransient<DetectedBrokerDepositProcessor>();
            services.AddTransient<ConfirmedDepositProcessor>();
            services.AddTransient<ConfirmedBrokerDepositProcessor>();
            services.AddTransient<ConfirmedDepositConsolidationProcessor>();

            return services;
        }
    }
}
