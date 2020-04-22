using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.Deposits.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain.Processing
{
    internal sealed class ProcessorsFactory : IProcessorsFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<Type> _detectedTransactionProcessors;
        private readonly List<Type> _confirmedTransactionProcessors;
        private readonly List<Type> _completedOperationProcessors;
        private readonly List<Type> _failedOperationProcessors;
        private readonly List<Type> _cancelledBlockProcessors;

        public ProcessorsFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _detectedTransactionProcessors = new List<Type>
            {
                typeof(DetectedDepositProcessor),
                typeof(DetectedBrokerDepositProcessor)
            };

            _confirmedTransactionProcessors = new List<Type>
            {
                typeof(ConfirmedDepositProcessor),
                typeof(ConfirmedBrokerDepositProcessor),
                typeof(ConfirmedDepositConsolidationProcessor)
            };
            
            _completedOperationProcessors = new List<Type>();
            _failedOperationProcessors = new List<Type>();
            _cancelledBlockProcessors = new List<Type>();
        }

        public IReadOnlyCollection<IDetectedTransactionProcessor> GetDetectedTransactionProcessors()
        {
            return _detectedTransactionProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<IDetectedTransactionProcessor>()
                .ToArray();
        }

        public IReadOnlyCollection<IConfirmedTransactionProcessor> GetConfirmedTransactionProcessors()
        {
            return _confirmedTransactionProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<IConfirmedTransactionProcessor>()
                .ToArray();
        }
        public IReadOnlyCollection<ICompletedOperationProcessor> GetCompletedOperationProcessors()
        {
            return _completedOperationProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<ICompletedOperationProcessor>()
                .ToArray();
        }

        public IReadOnlyCollection<IFailedOperationProcessor> GetFailedOperationProcessors()
        {
            return _failedOperationProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<IFailedOperationProcessor>()
                .ToArray();
        }

        public IReadOnlyCollection<ICancelledBlockProcessor> GetCancelledBlockProcessors()
        {
            return _cancelledBlockProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<ICancelledBlockProcessor>()
                .ToArray();
        }
    }
}
