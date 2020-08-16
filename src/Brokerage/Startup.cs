using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Brokerage.Common.Configuration;
using Brokerage.Common.Persistence;
using Brokerage.GrpcServices;
using Brokerage.HostedServices;
using Brokerage.Messaging;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.EfCore;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.Sdk.Crypto.AddressFormatting;

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

            services.AddPersistence(Config.Db.ConnectionString);
            services.AddHostedService<DbSchemaValidationHost>();
            services.AddIdempotency<UnitOfWork>(c =>
            {
                c.DispatchWithMassTransit();
                c.PersistWithEfCore(s =>
                {
                    var optionsBuilder = s.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();

                    return new DatabaseContext(optionsBuilder.Options);
                });
            });
            services.AddMessaging(Config.RabbitMq);

            services.AddTransient<IAddressFormatterFactory, AddressFormatterFactory>();
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
