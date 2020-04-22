using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedDepositConsolidationProcessor : IConfirmedTransactionProcessor
    {
        public Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.Withdrawal)
            {
                return Task.CompletedTask;
            }

            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var balancesContext in brokerAccountContext.Balances.Where(x => x.Income > 0))
                {
                    balancesContext.Balances.ConsolidateBalance(balancesContext.Income);
                }
            }

            return Task.CompletedTask;
        }
    }
}
