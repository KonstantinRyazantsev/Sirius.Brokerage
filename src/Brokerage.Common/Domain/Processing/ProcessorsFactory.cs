﻿using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.Deposits.Processors;
using Brokerage.Common.Domain.Withdrawals.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Brokerage.Common.Domain.Processing
{
    internal sealed class ProcessorsFactory : IProcessorsFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyCollection<Type> _detectedTransactionProcessors;
        private readonly IReadOnlyCollection<Type> _confirmedTransactionProcessors;
        private readonly IReadOnlyCollection<Type> _sentOperationProcessors;
        private readonly IReadOnlyCollection<Type> _completedOperationProcessors;
        private readonly IReadOnlyCollection<Type> _failedOperationProcessors;
        private readonly IReadOnlyCollection<Type> _cancelledBlockProcessors;
        private readonly IReadOnlyCollection<Type> _signingOperationProcessors;

        public ProcessorsFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _detectedTransactionProcessors = new List<Type>
            {
                typeof(DetectedDepositProcessor),
                typeof(DetectedBrokerDepositProcessor)
            };

            //Order of registration matters 
            _confirmedTransactionProcessors = new List<Type>
            {
                typeof(ConfirmedTinyDepositProcessor),
                typeof(ConfirmedTinyTokenDepositProcessor),
                typeof(ConfirmedRegularDepositProcessor),
                typeof(ConfirmedTokenDepositProcessor),
                typeof(ConfirmedBrokerDepositProcessor),
                typeof(ConfirmedDepositConsolidationProcessor)
            };

            _sentOperationProcessors = new List<Type>
            {
                typeof(SentWithdrawalProcessor)
            };

            _completedOperationProcessors = new List<Type>
            {
                typeof(CompletedRegularDepositProcessor),
                typeof(CompletedWithdrawalProcessor)
            };

            _failedOperationProcessors = new List<Type>
            {
                typeof(FailedDepositProcessor),
                typeof(FailedWithdrawalProcessor)
            };

            _signingOperationProcessors = new List<Type>()
            {
                typeof(SigningWithdrawalProcessor)
            };

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

        public IReadOnlyCollection<ISentOperationProcessor> GetSentOperationProcessors()
        {
            return _sentOperationProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<ISentOperationProcessor>()
                .ToArray();
        }

        public IReadOnlyCollection<ISigningOperationProcessor> GetSigningOperationProcessors()
        {
            return _signingOperationProcessors
                .Select(x => _serviceProvider.GetRequiredService(x))
                .Cast<ISigningOperationProcessor>()
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
