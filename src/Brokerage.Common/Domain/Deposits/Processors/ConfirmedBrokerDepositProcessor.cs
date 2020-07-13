using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedBrokerDepositProcessor : IConfirmedTransactionProcessor
    {
        public Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return Task.CompletedTask;
            }

            var brokerDeposits = processingContext.Deposits
                .Where(x => x.IsBrokerDeposit)
                .ToArray();

            foreach (var deposit in brokerDeposits)
            {
                deposit.ConfirmBroker(tx);
            }

            var balanceChanges = brokerDeposits
                .GroupBy(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                .Select(x => new
                {
                    Id = x.Key,
                    Amount = x.Sum(d => d.Unit.Amount)
                });

            foreach (var change in balanceChanges)
            {
                var balances = processingContext.BrokerAccountBalances[change.Id];

                balances.ConfirmBrokerPendingBalance(change.Amount);
            }

            return Task.CompletedTask;
        }
    }
}
