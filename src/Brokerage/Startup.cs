using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.HostedServices;
using Brokerage.Common.Persistence;
using Brokerage.Common.ServiceFunctions;
using Brokerage.GrpcServices;
using Brokerage.HostedServices;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.EfCore;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sdk.Server.Common;

namespace Brokerage
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services.AddOutbox(c =>
            {
                c.DispatchWithMassTransit();
                c.PersistWithEfCore(s =>
                {
                    var optionsBuilder = s.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();

                    return new DatabaseContext(optionsBuilder.Options);
                });
            });

            services.AddPersistence(Config.Db.ConnectionString);
            services.AddHostedService<DbSchemaValidationHost>();
            services.AddOutbox(c =>
            {
                c.DispatchWithMassTransit();
                c.PersistWithEfCore(s =>
                {
                    var optionsBuilder = s.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();

                    return new DatabaseContext(optionsBuilder.Options);
                });
            });
            services.AddMassTransit(x =>
            {
                EndpointConvention.Map<FinalizeBrokerAccountCreation>(
                    new Uri("queue:sirius-brokerage-finalize-broker-account-creation"));
                EndpointConvention.Map<FinalizeAccountCreation>(
                    new Uri("queue:sirius-brokerage-finalize-account-creation"));
                EndpointConvention.Map<PublishAccountDetails>(
                    new Uri("queue:sirius-brokerage-publish-account-details"));
                EndpointConvention.Map<ExecuteWithdrawal>(new Uri("queue:sirius-brokerage-execute-withdrawal"));

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(Config.RabbitMq.HostUrl, host =>
                    {
                        host.Username(Config.RabbitMq.Username);
                        host.Password(Config.RabbitMq.Password);
                    });
            
                    cfg.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
                }));
            
                services.AddSingleton<IHostedService, BusHost>();
            });
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            endpoints.MapGrpcService<MonitoringService>();
            endpoints.MapGrpcService<BrokerAccountsService>();
            endpoints.MapGrpcService<AccountsService>();
            endpoints.MapGrpcService<WithdrawalsService>();
        }
    }
}
