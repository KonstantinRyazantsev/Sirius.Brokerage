using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IProcessorsProvider, ProcessorsProvider>();
            services.AddTransient<DepositsDetector>();
            services.AddTransient<BalanceUpdateConfirmator>();
            services.AddTransient<DepositsConfirmator>();
            
            return services;
        }
    }
}
