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
                    OperationErrorCode.ValidationRejected => WithdrawalErrorCode.ValidationRejected,
                    OperationErrorCode.SigningRejected => WithdrawalErrorCode.SigningRejected,
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

            var actualFees = evt.ActualFees != null
                ? FeesMath.SpreadAcrossBrokerAccounts(evt.ActualFees, brokerAccountIds)
                : new Dictionary<BrokerAccountBalancesId, decimal>();

            var expectedFees = processingContext.Operation.ExpectedFees != null
                ? FeesMath.SpreadAcrossBrokerAccounts(processingContext.Operation.ExpectedFees, brokerAccountIds)
                : new Dictionary<BrokerAccountBalancesId, decimal>();

            var balanceChanges = processingContext.Withdrawals
                .GroupBy(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                .ToDictionary(
                    g => g.Key,
                    g => (Amount: g.Sum(x => x.Unit.Amount),
                        ActualFee: actualFees.GetValueOrDefault(g.Key),
                        ExpectedFee: expectedFees.GetValueOrDefault(g.Key)));

            foreach (var (balanceId, value) in balanceChanges.Where(x => x.Value.Amount > 0))
            {
                var balances = processingContext.BrokerAccountBalances[balanceId];

                balances.FailWithdrawal(value.Amount, value.ActualFee, value.ExpectedFee);
            }

            return Task.CompletedTask;
        }
    }
}
