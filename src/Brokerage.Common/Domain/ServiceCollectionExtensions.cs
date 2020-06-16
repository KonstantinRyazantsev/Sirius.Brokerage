using Brokerage.Common.Domain.Deposits.Processors;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Domain.Withdrawals.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddSingleton<IProcessorsFactory, ProcessorsFactory>();
            services.AddTransient<IOperationsExecutor, OperationsExecutor>();

            services.AddTransient<TransactionProcessingContextBuilder>();
            services.AddTransient<OperationProcessingContextBuilder>();

            services.AddTransient<DetectedDepositProcessor>();
            services.AddTransient<DetectedBrokerDepositProcessor>();
            services.AddTransient<ConfirmedDepositProcessor>();
            services.AddTransient<ConfirmedBrokerDepositProcessor>();
            services.AddTransient<ConfirmedDepositConsolidationProcessor>();
            services.AddTransient<SentWithdrawalProcessor>();
            services.AddTransient<CompletedDepositProcessor>();
            services.AddTransient<CompletedWithdrawalProcessor>();
            services.AddTransient<FailedDepositProcessor>();
            services.AddTransient<FailedWithdrawalProcessor>();
            services.AddSingleton<IDestinationTagGeneratorFactory, DestinationTagGeneratorFactory>();

            return services;
        }
    }
}
