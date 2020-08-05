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
    public class CompletedWithdrawalProcessor : ICompletedOperationProcessor
    {
        public Task Process(OperationCompleted evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.Withdrawal)
            {
                return Task.CompletedTask;
            }

            foreach (var withdrawal in processingContext.Withdrawals)
            {
                withdrawal.Complete();
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

                balances.FreeReservedBalance(value.ExpectedFee - value.ActualFee);
                balances.Withdraw(value.Amount + value.ActualFee);
            }

            return Task.CompletedTask;
        }
    }
}
