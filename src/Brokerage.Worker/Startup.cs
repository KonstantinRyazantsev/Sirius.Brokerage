using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Brokerage.Common.Configuration;
using Brokerage.Common.HostedServices;
using Brokerage.Common.Persistence;
using Brokerage.Worker.HostedServices;
using Brokerage.Worker.MessageConsumers;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.VaultAgent.ApiClient;

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
            services.AddTransient<IVaultAgentClient>(x => new VaultAgentClient(Config.VaultAgent.Url));
            services.AddPersistence(Config.Db.ConnectionString);
            services.AddHostedService<MigrationHost>();
            services.AddMessageConsumers();
            
            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(Config.RabbitMq.HostUrl, host =>
                    {
                        host.Username(Config.RabbitMq.Username);
                        host.Password(Config.RabbitMq.Password);
                    });
            
                    cfg.UseMessageRetry(y =>
                        y.Exponential(5, 
                            TimeSpan.FromMilliseconds(100),
                            TimeSpan.FromMilliseconds(10_000), 
                            TimeSpan.FromMilliseconds(100)));
            
                    cfg.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
                    
                    cfg.ReceiveEndpoint("sirius-brokerage-blockchain-updates", e =>
                    {
                        e.Consumer(provider.GetRequiredService<BlockchainUpdatesConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-finalize-broker-account-creation", e =>
                    {
                        e.Consumer(provider.GetRequiredService<FinalizeBrokerAccountCreationConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-finalize-account-creation", e =>
                    {
                        e.Consumer(provider.GetRequiredService<FinalizeAccountCreationConsumer>);
                    });
                }));
            
                services.AddHostedService<BusHost>();
            });
        }
    }
}
