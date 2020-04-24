using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class FailedDepositProcessor : IFailedOperationProcessor
    {
        public Task Process(OperationFailed evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.DepositConsolidation)
            {
                return Task.CompletedTask;
            }

            foreach (var deposit in processingContext.Deposits)
            {
                deposit.Fail(new DepositError(evt.ErrorMessage,
                    evt.ErrorCode switch
                    {
                        OperationErrorCode.TechnicalProblem => DepositErrorCode.TechnicalProblem,
                        OperationErrorCode.NotEnoughBalance => DepositErrorCode.TechnicalProblem,
                        OperationErrorCode.InvalidDestinationAddress => DepositErrorCode.TechnicalProblem,
                        OperationErrorCode.DestinationTagRequired => DepositErrorCode.TechnicalProblem,
                        OperationErrorCode.AmountIsTooSmall => DepositErrorCode.TechnicalProblem,

                        _ => throw new ArgumentOutOfRangeException(nameof(evt.ErrorCode), evt.ErrorCode, null)
                    }));
            }

            return Task.CompletedTask;
        }
    }
}
