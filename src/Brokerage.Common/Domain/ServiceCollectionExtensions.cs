using Brokerage.Common.Domain.Deposits;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddTransient<DepositsDetector>();

            return services;
        }
    }
}
