using Brokerage.Bilv1.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Bilv1.Repositories
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IWalletRepository, WalletRepository>();
            services.AddTransient<IEnrolledBalanceRepository, EnrolledBalanceRepository>();
            services.AddTransient<IOperationRepository, OperationRepository>();

            return services;
        }
    }
}
