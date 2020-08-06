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
