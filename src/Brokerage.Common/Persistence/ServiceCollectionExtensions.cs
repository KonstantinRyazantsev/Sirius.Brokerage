using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.Persistence.Blockchains;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brokerage.Common.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IBlockchainsRepository, BlockchainsRepository>();
            services.AddTransient<IAssetsRepository, AssetsRepository>();

            services.AddSingleton<DbContextOptionsBuilder<DatabaseContext>>(x =>
            {
                var loggerFactory = x.GetRequiredService<ILoggerFactory>();
                var optionsBuilder = CreateDbContextOptionsBuilder(connectionString, loggerFactory);

                return optionsBuilder;
            });

            return services;
        }

        private static DbContextOptionsBuilder<DatabaseContext> CreateDbContextOptionsBuilder(string connectionString, ILoggerFactory loggerFactory)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseLoggerFactory(loggerFactory);

            optionsBuilder.UseNpgsql(connectionString,
                builder =>
                    builder.MigrationsHistoryTable(
                        DatabaseContext.MigrationHistoryTable,
                        DatabaseContext.SchemaName));
            return optionsBuilder;
        }
    }
}
