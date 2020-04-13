using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Common.Domain.Deposits
{
    public class DetectedBrokerDepositProcessor : IDetectedTransactionProcessor
    {
        private readonly IOutboxManager _outboxManager;

        public DetectedBrokerDepositProcessor(IOutboxManager outboxManager)
        {
            _outboxManager = outboxManager;
        }

        public async Task Process(TransactionDetected tx, ProcessingContext processingContext)
        {
            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var input in brokerAccountContext.Inputs)
                {
                    var outbox = await _outboxManager.Open(
                        $"Deposits:Create:{input.Requisites.Id}_{input.Unit.AssetId}_{tx.TransactionId}", 
                        OutboxAggregateIdGenerator.Sequential);

                    var deposit = Deposit.Create(
                        outbox.AggregateId,
                        input.Requisites.Id,
                        null,
                        input.Unit,
                        processingContext.TransactionInfo,
                        tx.Sources
                            .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                            .ToArray());
                }

                foreach (var balancesContext in brokerAccountContext.Balances.Where(x => x.Income > 0))
                {
                    balancesContext.Balances.AddPendingBalance(balancesContext.Income);
                }
            }
        }
    }
}
