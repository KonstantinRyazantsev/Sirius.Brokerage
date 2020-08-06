using System;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Worker.Messaging.Consumers;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Swisschain.Extensions.MassTransit;

namespace Brokerage.Worker.Messaging
{
    public static class MassTransitServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, RabbitMqConfig rabbitMqConfig)
        {
            services.AddTransient<BlockchainUpdatedConsumer>();
            services.AddTransient<FinalizeBrokerAccountCreationConsumer>();
            services.AddTransient<FinalizeAccountCreationConsumer>();
            services.AddTransient<PublishAccountDetailsConsumer>();
            services.AddTransient<TransactionDetectedConsumer>();
            services.AddTransient<TransactionConfirmedConsumer>();
            services.AddTransient<OperationSentConsumer>();
            services.AddTransient<OperationCompletedConsumer>();
            services.AddTransient<OperationFailedConsumer>();
            services.AddTransient<AssetAddedConsumer>();
            services.AddTransient<ExecuteWithdrawalConsumer>();
            services.AddTransient<WalletAddedConsumer>();
            services.AddTransient<CreateAccountDetailsForTagConsumer>();

            ConfigureCommands();

            services.AddMassTransit(x =>
            {
                var schedulerEndpoint = new Uri("queue:sirius-pulsar");

                x.AddMessageScheduler(schedulerEndpoint);
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConfig.HostUrl, host =>
                    {
                        host.Username(rabbitMqConfig.Username);
                        host.Password(rabbitMqConfig.Password);
                    });

                    cfg.UseMessageScheduler(schedulerEndpoint);

                    cfg.UseDefaultRetries();

                    ConfigureReceivingEndpoints(cfg, context);
                });
            });

            services.AddMassTransitBusHost();

            return services;
        }

        private static void ConfigureCommands()
        {
            EndpointConvention.Map<CreateAccountDetailsForTag>(new Uri("queue:sirius-brokerage-create-account-details-for-tag"));
        }

        private static void ConfigureReceivingEndpoints(IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
        {
            cfg.ReceiveEndpoint("sirius-brokerage-blockchain-updated", e =>
            {
                e.Consumer(context.GetRequiredService<BlockchainUpdatedConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-finalize-broker-account-creation", e =>
            {
                e.Consumer(context.GetRequiredService<FinalizeBrokerAccountCreationConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-create-account-details-for-tag", e =>
            {
                e.Consumer(context.GetRequiredService<CreateAccountDetailsForTagConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-finalize-account-creation", e =>
            {
                e.Consumer(context.GetRequiredService<FinalizeAccountCreationConsumer>);
            });
            
            cfg.ReceiveEndpoint("sirius-brokerage-publish-account-details", e =>
            {
                e.Consumer(context.GetRequiredService<PublishAccountDetailsConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-transaction-detection", e =>
            {
                e.PrefetchCount = 100;
                e.UseConcurrencyLimit(4);
                e.Consumer(context.GetRequiredService<TransactionDetectedConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-transaction-confirmation", e =>
            {
                e.PrefetchCount = 100;
                e.UseConcurrencyLimit(4);
                e.Consumer(context.GetRequiredService<TransactionConfirmedConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-operation-sending", e =>
            {
                e.Consumer(context.GetRequiredService<OperationSentConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-operation-completion", e =>
            {
                e.Consumer(context.GetRequiredService<OperationCompletedConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-operation-failure", e =>
            {
                e.Consumer(context.GetRequiredService<OperationFailedConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-asset-added", e =>
            {
                e.Consumer(context.GetRequiredService<AssetAddedConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-execute-withdrawal", e =>
            {
                e.Consumer(context.GetRequiredService<ExecuteWithdrawalConsumer>);
            });

            cfg.ReceiveEndpoint("sirius-brokerage-wallet-added", e =>
            {
                e.Consumer(context.GetRequiredService<WalletAddedConsumer>);
            });
        }
    }
}
