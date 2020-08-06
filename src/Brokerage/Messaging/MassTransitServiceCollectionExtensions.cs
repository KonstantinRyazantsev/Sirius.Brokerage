using System;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.ServiceFunctions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Swisschain.Extensions.MassTransit;

namespace Brokerage.Messaging
{
    public static class MassTransitServiceCollectionExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, RabbitMqConfig rabbitMqConfig)
        {
            ConfigureCommands();

            services.AddMassTransit(x =>
            {
                var schedulerEndpoint = new Uri("queue:sirius-pulsar");

                x.AddMessageScheduler(schedulerEndpoint);

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqConfig.HostUrl,
                        host =>
                        {
                            host.Username(rabbitMqConfig.Username);
                            host.Password(rabbitMqConfig.Password);
                        });

                    cfg.UseMessageScheduler(schedulerEndpoint);

                    cfg.UseDefaultRetries();
                });
            });

            services.AddMassTransitBusHost();

            return services;
        }

        private static void ConfigureCommands()
        {
            EndpointConvention.Map<FinalizeBrokerAccountCreation>(new Uri("queue:sirius-brokerage-finalize-broker-account-creation"));
            EndpointConvention.Map<FinalizeAccountCreation>(new Uri("queue:sirius-brokerage-finalize-account-creation"));
            EndpointConvention.Map<PublishAccountDetails>(new Uri("queue:sirius-brokerage-publish-account-details"));
            EndpointConvention.Map<ExecuteWithdrawal>(new Uri("queue:sirius-brokerage-execute-withdrawal"));
            EndpointConvention.Map<CreateAccountDetailsForTag>(new Uri("queue:sirius-brokerage-create-account-details-for-tag"));
        }
    }
}
