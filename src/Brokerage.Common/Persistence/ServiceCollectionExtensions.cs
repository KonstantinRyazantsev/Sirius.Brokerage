using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IBrokerAccountsRepository, BrokerAccountsRepository>();
            services.AddTransient<IBrokerAccountRequisitesRepository, BrokerAccountRequisitesRepository>();
            services.AddTransient<IAccountsRepository, AccountsRepository>();
            services.AddTransient<IAccountRequisitesRepository, AccountRequisitesRepository>();
            services.AddTransient<IBlockchainsRepository, BlockchainsRepository>();

            services.AddSingleton<DbContextOptionsBuilder<DatabaseContext>>(x =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
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
