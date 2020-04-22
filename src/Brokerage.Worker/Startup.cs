using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain;
using Brokerage.Common.HostedServices;
using Brokerage.Common.Persistence;
using Brokerage.Worker.HostedServices;
using Brokerage.Worker.MessageConsumers;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Brokerage.Common.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.EfCore;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Executor.ApiClient;

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
            services.AddTransient<IExecutorClient>(x => new ExecutorClient(Config.Executor.Url));
            services.AddPersistence(Config.Db.ConnectionString);
            services.AddOutbox(c =>
            {
                c.DispatchWithMassTransit();
                c.PersistWithEfCore(s =>
                {
                    var optionsBuilder = s.GetRequiredService<DbContextOptionsBuilder<DatabaseContext>>();

                    return new DatabaseContext(optionsBuilder.Options);
                });
            });
            services.AddHostedService<MigrationHost>();
            services.AddDomain();

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
                    
                    cfg.ReceiveEndpoint("sirius-brokerage-publish-account-requisites", e =>
                    {
                        e.Consumer(provider.GetRequiredService<PublishAccountRequisitesConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-transaction-detection", e =>
                    {
                        e.Consumer(provider.GetRequiredService<TransactionDetectedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-transaction-confirmation", e =>
                    {
                        e.Consumer(provider.GetRequiredService<TransactionConfirmedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-operation-completion", e =>
                    {
                        e.Consumer(provider.GetRequiredService<OperationCompletedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-operation-failure", e =>
                    {
                        e.Consumer(provider.GetRequiredService<OperationFailedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-asset-added", e =>
                    {
                        e.Consumer(provider.GetRequiredService<AssetAddedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-execute-withdrawal", e =>
                    {
                        e.Consumer(provider.GetRequiredService<ExecuteWithdrawalConsumer>);
                    });
                }));
            
                services.AddHostedService<BusHost>();
            });
        }
    }
}
