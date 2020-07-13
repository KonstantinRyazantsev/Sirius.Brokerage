using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedDepositConsolidationProcessor : IConfirmedTransactionProcessor
    {
        public Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type != OperationType.DepositConsolidation)
            {
                return Task.CompletedTask;
            }

            if (processingContext.Deposits.Count != 1)
            {
                throw new InvalidOperationException("Only one deposit per consolidation transaction is supported for now");
            }

            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var balancesContext in brokerAccountContext.Balances)
                {
                    var assetId = balancesContext.AssetId;
                    var fee = tx.Fees.SingleOrDefault(x => x.AssetId == assetId)?.Amount ?? 0m;
                    
                    balancesContext.Balances.ConsolidateBalance(balancesContext.Income, fee);
                }
            }

            return Task.CompletedTask;
        }
    }
}
