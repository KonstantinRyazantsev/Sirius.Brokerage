using System.Linq;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Transactions;
using Brokerage.Common.Persistence.Withdrawals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Z.EntityFramework.Extensions;

namespace Brokerage.Common.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IBrokerAccountsRepository, BrokerAccountsRepository>();
            services.AddTransient<IBrokerAccountDetailsRepository, BrokerAccountDetailsRepository>();
            services.AddTransient<IBrokerAccountsBalancesRepository, BrokerAccountsBalancesRepository>();
            services.AddTransient<IAccountsRepository, AccountsRepository>();
            services.AddTransient<IAccountDetailsRepository, AccountDetailsRepository>();
            services.AddTransient<IBlockchainsRepository, BlockchainsRepository>();
            services.AddTransient<IDepositsRepository, DepositsRepository>();
            services.AddTransient<IWithdrawalRepository, WithdrawalRepository>();
            services.AddTransient<IAssetsRepository, AssetsRepository>();
            services.AddTransient<IOperationsRepository, OperationsRepository>();
            services.AddTransient<IDetectedTransactionsRepository, DetectedTransactionsRepository>();

            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            var loggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider(optionsMonitor) }, new LoggerFilterOptions { MinLevel = LogLevel.Information });

            services.AddSingleton<DbContextOptionsBuilder<DatabaseContext>>(x =>
            {
                var optionsBuilder = CreateDbContextOptionsBuilder(connectionString);

                return optionsBuilder;
            });

            EntityFrameworkManager.ContextFactory = context =>
            {
                var optionsBuilder = CreateDbContextOptionsBuilder(connectionString);
                return new DatabaseContext(optionsBuilder.Options);
            };

            return services;
        }

        private static DbContextOptionsBuilder<DatabaseContext> CreateDbContextOptionsBuilder(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            //optionsBuilder.UseLoggerFactory(loggerFactory) //tie-up DbContext with LoggerFactory object
            //    .EnableSensitiveDataLogging();
            optionsBuilder.UseNpgsql(connectionString,
                builder =>
                    builder.MigrationsHistoryTable(
                        DatabaseContext.MigrationHistoryTable,
                        DatabaseContext.SchemaName));
            return optionsBuilder;
        }
    }
}
