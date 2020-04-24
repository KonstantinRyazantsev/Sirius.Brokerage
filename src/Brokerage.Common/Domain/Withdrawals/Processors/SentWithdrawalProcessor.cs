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
    public class SentWithdrawalProcessor : ISentOperationProcessor
    {
        public Task Process(OperationSent evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.Withdrawal)
            {
                return Task.CompletedTask;
            }

            foreach (var withdrawal in processingContext.Withdrawals)
            {
                withdrawal.TrackSent();
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
                
                balances.ReserveBalance(value);
            }

            return Task.CompletedTask;
        }
    }
}
