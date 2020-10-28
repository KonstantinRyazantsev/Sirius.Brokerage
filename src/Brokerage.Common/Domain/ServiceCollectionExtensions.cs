using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Deposits.Processors;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Domain.Withdrawals.Processors;
using Microsoft.Extensions.DependencyInjection;
using Swisschain.Sirius.Sdk.Crypto.AddressFormatting;

namespace Brokerage.Common.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IProcessorsFactory, ProcessorsFactory>();
            services.AddTransient<IOperationsFactory, OperationsFactory>();

            services.AddTransient<TransactionProcessingContextBuilder>();
            services.AddTransient<OperationProcessingContextBuilder>();

            services.AddTransient<DetectedDepositProcessor>();
            services.AddTransient<DetectedBrokerDepositProcessor>();
            services.AddTransient<ConfirmedRegularDepositProcessor>();
            services.AddTransient<ConfirmedTokenDepositProcessor>();
            services.AddTransient<ConfirmedTinyDepositProcessor>();
            services.AddTransient<ConfirmedTinyTokenDepositProcessor>();
            services.AddTransient<ConfirmedBrokerDepositProcessor>();
            services.AddTransient<ConfirmedDepositConsolidationProcessor>();
            services.AddTransient<SentWithdrawalProcessor>();
            services.AddTransient<CompletedRegularDepositProcessor>();
            services.AddTransient<CompletedWithdrawalProcessor>();
            services.AddTransient<SigningWithdrawalProcessor>();
            services.AddTransient<FailedDepositProcessor>();
            services.AddTransient<FailedWithdrawalProcessor>();
            services.AddSingleton<IDestinationTagGeneratorFactory, DestinationTagGeneratorFactory>();
            services.AddSingleton<IAddressFormatterFactory, AddressFormatterFactory>();
            services.AddSingleton<IDepositFactory, DepositFactory>();

            return services;
        }
    }
}
