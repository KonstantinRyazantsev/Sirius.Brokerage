﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Brokerage.Common.Configuration;
using Brokerage.Common.Persistence;
using Brokerage.GrpcServices;
using Brokerage.HostedServices;
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

            services.AddPersistence(Config.Db.ConnectionString);
            services.AddHostedService<DbSchemaValidationHost>();
            //services.AddMassTransit(x =>
            //{
            //    // TODO: Register commands recipient endpoints. It's just an example.
            //    EndpointConvention.Map<ExecuteSomething>(new Uri("queue:sirius-brokerage-something-execution"));
            //
            //    x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            //    {
            //        cfg.Host(Config.RabbitMq.HostUrl, host =>
            //        {
            //            host.Username(Config.RabbitMq.Username);
            //            host.Password(Config.RabbitMq.Password);
            //        });
            //
            //        cfg.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
            //    }));
            //
            //    services.AddSingleton<IHostedService, BusHost>();
            //});
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            endpoints.MapGrpcService<MonitoringService>();
            endpoints.MapGrpcService<BrokerAccountService>();
        }
    }
}
