using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain;
using Brokerage.Common.Limiters;
using Brokerage.Common.Persistence;
using Brokerage.Worker.HostedServices;
using Brokerage.Worker.Messaging;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.EfCore;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Extensions.Postgres;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Sdk.Crypto.AddressFormatting;

namespace Brokerage.Worker
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services.AddHttpClient();
            services.AddSingleton<ConcurrencyLimiter>(new ConcurrencyLimiter(1));
            services.AddTransient<IVaultAgentClient>(x => new VaultAgentClient(Config.VaultAgent.Url));
            services.AddTransient<IExecutorClient>(x => new ExecutorClient(Config.Executor.Url));
            services.AddTransient<IAddressFormatterFactory, AddressFormatterFactory>();
            services.AddPersistence(Config.Db.ConnectionString);
            services.AddIdempotency<UnitOfWork>(c =>
            {
                c.DispatchWithMassTransit();
                c.PersistWithEfCore(s =>
                {
                    var optionsBuilder = s.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();

                    return new DatabaseContext(optionsBuilder.Options);
                });
            });
            services.AddStaleConnectionsCleaning(Config.Db.ConnectionString, TimeSpan.FromMinutes(5));
            services.AddHostedService<MigrationHost>();
            services.AddDomain();
            services.AddMessaging(Config.RabbitMq);
        }
    }
}
