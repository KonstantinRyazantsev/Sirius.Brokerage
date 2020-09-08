using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class CompletedDepositProcessor : ICompletedOperationProcessor
    {
        public Task Process(OperationCompleted evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.DepositConsolidation)
            {
                return Task.CompletedTask;
            }

            //TODO: Fix it? Or should we?
            foreach (var minDeposit in processingContext.MinDeposits)
            {
                minDeposit.Complete(Array.Empty<Unit>());
            }

            foreach (var deposit in processingContext.Deposits)
            {
                deposit.Complete(evt.ActualFees);
            }

            return Task.CompletedTask;
        }
    }
}
