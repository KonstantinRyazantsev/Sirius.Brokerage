using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Operations;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Common.Domain.Withdrawals.Processors
{
    public class SentWithdrawalProcessor : ISentOperationProcessor
    {
        private readonly IOperationsRepository _operationsRepository;

        public SentWithdrawalProcessor(IOperationsRepository operationsRepository)
        {
            _operationsRepository = operationsRepository;
        }

        public Task Process(OperationSent evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.Withdrawal)
            {
                return Task.CompletedTask;
            }

            var operation = processingContext.Operation;

            foreach (var withdrawal in processingContext.Withdrawals)
            {
                withdrawal.TrackSent();
            }

            var brokerAccountIds = processingContext.Withdrawals
                .Select(x => x.BrokerAccountId)
                .Distinct()
                .ToArray();

            operation.AddExpectedFees(evt.ExpectedFees);

            var fees = evt.ExpectedFees != null
                    ? FeesMath.SpreadAcrossBrokerAccounts(evt.ExpectedFees, brokerAccountIds)
                    : new Dictionary<BrokerAccountBalancesId, decimal>();

            foreach (var (balanceId, value) in fees.Where(x => x.Value > 0))
            {
                if (!processingContext.BrokerAccountBalances.TryGetValue(balanceId, out var balances))
                {
                    continue;
                }

                balances.ReserveBalance(value);
            }

            return Task.CompletedTask;
        }
    }
}
