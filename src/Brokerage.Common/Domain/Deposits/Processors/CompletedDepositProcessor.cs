using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;

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

            foreach (var deposit in processingContext.Deposits)
            {
                deposit.Complete();    
            }

            return Task.CompletedTask;
        }
    }
}
