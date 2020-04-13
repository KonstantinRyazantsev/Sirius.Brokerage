using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Common.Domain.Deposits
{
    public class DetectedDepositProcessor : IDetectedTransactionProcessor
    {
        public async Task Process(TransactionDetected tx, ProcessingContext processingContext)
        {
            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var accountContext in brokerAccountContext.Accounts)
                {
                    foreach (var input in accountContext.Inputs)
                    {
                        var deposit = Deposit.Create(
                            0,
                            // TODO: Get active BA Req or replace it with Broker account ID?
                            -1,
                            accountContext.Requisites.Id,
                            input,
                            processingContext.TransactionInfo,
                            tx.Sources
                                .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                                .ToArray());
                    }
                }

                foreach (var balancesContext in brokerAccountContext.Balances.Where(x => x.Income > 0))
                {
                    balancesContext.Balances.AddPendingBalance(balancesContext.Income);
                }
            }
        }
    }
}
