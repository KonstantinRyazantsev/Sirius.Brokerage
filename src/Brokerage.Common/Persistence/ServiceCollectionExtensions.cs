using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IBrokerAccountRepository, BrokerAccountRepository>();
            services.AddTransient<IBrokerAccountRequisitesRepository, BrokerAccountRequisitesRepository>();
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IAccountRequisitesRepository, AccountRequisitesRepository>();

            services.AddTransient<IProtocolReadModelRepository, ProtocolReadModelRepository>();
            services.AddTransient<IBlockchainReadModelRepository, BlockchainReadModelRepository>();

            services.AddSingleton<DbContextOptionsBuilder<BrokerageContext>>(x =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<BrokerageContext>();
                optionsBuilder.UseNpgsql(connectionString,
                    builder =>
                        builder.MigrationsHistoryTable(
                            PostgresRepositoryConfiguration.MigrationHistoryTable,
                            PostgresRepositoryConfiguration.SchemaName));

                return optionsBuilder;
            });

            return services;
        }
    }
}
