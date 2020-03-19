using System;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain.AppFeatureExample;
using Brokerage.Common.HostedServices;
using Brokerage.Common.Persistence;
using Brokerage.GrpcServices;
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
            services.AddAppFeatureExample();
            
            services.AddMassTransit(x =>
            {
                // TODO: Register commands recipient endpoints. It's just an example.
                EndpointConvention.Map<ExecuteSomething>(new Uri("queue:sirius-brokerage-something-execution"));

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(Config.RabbitMq.HostUrl, host =>
                    {
                        host.Username(Config.RabbitMq.Username);
                        host.Password(Config.RabbitMq.Password);
                    });

                    cfg.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
                }));

                services.AddHostedService<BusHost>();
            });
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            endpoints.MapGrpcService<MonitoringService>();
        }
    }
}
