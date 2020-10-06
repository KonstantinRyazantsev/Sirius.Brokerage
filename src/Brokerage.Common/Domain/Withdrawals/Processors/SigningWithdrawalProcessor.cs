using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Common.Domain.Withdrawals.Processors
{
    public class SigningWithdrawalProcessor : ISigningOperationProcessor
    {
        public Task Process(OperationSigning evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.Withdrawal)
            {
                return Task.CompletedTask;
            }

            foreach (var withdrawal in processingContext.Withdrawals)
            {
                withdrawal.MoveToSigning();
            }

            return Task.CompletedTask;
        }
    }

    //public class ValidatinWithdrawalProcessor : IValidatingOperationProcessor
    //{
    //    public Task Process(OperationValidating evt, OperationProcessingContext processingContext)
    //    {
    //        if (processingContext.Operation.Type != OperationType.Withdrawal)
    //        {
    //            return Task.CompletedTask;
    //        }

    //        foreach (var withdrawal in processingContext.Withdrawals)
    //        {
    //            withdrawal.();
    //        }

    //        return Task.CompletedTask;
    //    }
    //}
}
