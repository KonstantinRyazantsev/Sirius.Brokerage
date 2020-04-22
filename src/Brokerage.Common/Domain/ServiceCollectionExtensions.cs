using Brokerage.Common.Domain.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IProcessorsFactory, ProcessorsFactory>();
            
            return services;
        }
    }
}
