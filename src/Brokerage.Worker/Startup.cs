using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence;
using Brokerage.Worker.HostedServices;
using Brokerage.Worker.MessageConsumers;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Microsoft.EntityFrameworkCore;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.EfCore;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Extensions.MassTransit;
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
            services.AddTransient<IVaultAgentClient>(x => new VaultAgentClient(Config.VaultAgent.Url));
            services.AddTransient<IExecutorClient>(x => new ExecutorClient(Config.Executor.Url));
            services.AddTransient<IAddressFormatterFactory, AddressFormatterFactory>();
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

            services.AddStaleConnectionsCleaning(Config.Db.ConnectionString, TimeSpan.FromMinutes(5));
            services.AddHostedService<MigrationHost>();
            services.AddDomain();

            services.AddMessageConsumers();

            services.AddMassTransit(x =>
            {
                EndpointConvention.Map<CreateAccountDetailsForTag>(
                    new Uri("queue:sirius-brokerage-create-account-details-for-tag"));

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(Config.RabbitMq.HostUrl, host =>
                    {
                        host.Username(Config.RabbitMq.Username);
                        host.Password(Config.RabbitMq.Password);
                    });
            
                    cfg.UseMessageRetry(y =>
                    {
                        y.AddRetriesAudit(provider.Container);

                        y.Exponential(5,
                            TimeSpan.FromMilliseconds(100),
                            TimeSpan.FromMilliseconds(10_000),
                            TimeSpan.FromMilliseconds(100));
                    });
            
                    cfg.SetLoggerFactory(provider.Container.GetRequiredService<ILoggerFactory>());
                    
                    cfg.ReceiveEndpoint("sirius-brokerage-blockchain-updated", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<BlockchainUpdatedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-finalize-broker-account-creation", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<FinalizeBrokerAccountCreationConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-create-account-details-for-tag", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<CreateAccountDetailsForTagConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-finalize-account-creation", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<FinalizeAccountCreationConsumer>);
                    });
                    
                    cfg.ReceiveEndpoint("sirius-brokerage-publish-account-details", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<PublishAccountDetailsConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-transaction-detection", e =>
                    {
                        e.PrefetchCount = 100;
                        e.UseConcurrencyLimit(4);
                        e.Consumer(provider.Container.GetRequiredService<TransactionDetectedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-transaction-confirmation", e =>
                    {
                        e.PrefetchCount = 100;
                        e.UseConcurrencyLimit(4);
                        e.Consumer(provider.Container.GetRequiredService<TransactionConfirmedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-operation-sending", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<OperationSentConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-operation-completion", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<OperationCompletedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-operation-failure", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<OperationFailedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-asset-added", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<AssetAddedConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-execute-withdrawal", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<ExecuteWithdrawalConsumer>);
                    });

                    cfg.ReceiveEndpoint("sirius-brokerage-wallet-added", e =>
                    {
                        e.Consumer(provider.Container.GetRequiredService<WalletAddedConsumer>);
                    });

                }));
            });

            services.AddMassTransitBusHost();
        }
    }
}
