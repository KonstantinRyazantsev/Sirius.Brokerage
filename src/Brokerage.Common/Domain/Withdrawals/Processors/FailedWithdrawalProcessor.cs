using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Common.Domain.Withdrawals.Processors
{
    public class FailedWithdrawalProcessor : IFailedOperationProcessor
    {
        public Task Process(OperationFailed evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.Withdrawal)
            {
                return Task.CompletedTask;
            }

            var error = new WithdrawalError(
                evt.ErrorMessage,
                evt.ErrorCode switch
                {
                    OperationErrorCode.TechnicalProblem => WithdrawalErrorCode.TechnicalProblem,
                    OperationErrorCode.NotEnoughBalance => WithdrawalErrorCode.NotEnoughBalance,
                    OperationErrorCode.InvalidDestinationAddress => WithdrawalErrorCode.InvalidDestinationAddress,
                    OperationErrorCode.DestinationTagRequired => WithdrawalErrorCode.DestinationTagRequired,
                    OperationErrorCode.AmountIsTooSmall => WithdrawalErrorCode.NotEnoughBalance,
                    _ => throw new ArgumentOutOfRangeException(nameof(evt.ErrorCode), evt.ErrorCode, "")
                });

            foreach (var withdrawal in processingContext.Withdrawals)
            {
                withdrawal.Fail(error);
            }

            var brokerAccountIds = processingContext.Withdrawals
                .Select(x => x.BrokerAccountId)
                .Distinct()
                .ToArray();

            var fees = evt.ActualFees != null
                ? FeesMath.SpreadAcrossBrokerAccounts(evt.ActualFees, brokerAccountIds)
                : new Dictionary<BrokerAccountBalancesId, decimal>();

            foreach (var (balanceId, value) in fees.Where(x => x.Value > 0))
            {
                var balances = processingContext.BrokerAccountBalances[balanceId];
                
                balances.Withdraw(value);
            }

            return Task.CompletedTask;
        }
    }
}
