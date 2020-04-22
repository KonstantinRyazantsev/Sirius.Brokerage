using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Deposits;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class DetectedDepositProcessor : IDetectedTransactionProcessor
    {
        private readonly IOutboxManager _outboxManager;
        private readonly IDepositsRepository _depositsRepository;

        public DetectedDepositProcessor(IOutboxManager outboxManager,
            IDepositsRepository depositsRepository)
        {
            _outboxManager = outboxManager;
            _depositsRepository = depositsRepository;
        }

        public async Task Process(TransactionDetected tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return;
            }

            var deposits = new List<Deposit>();

            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var accountContext in brokerAccountContext.Accounts)
                {
                    foreach (var input in accountContext.Inputs.Where(x => x.Amount > 0))
                    {
                        var outbox = await _outboxManager.Open(
                            $"Deposit:Create:{tx.TransactionId}-{accountContext.Requisites.Id}-{input.AssetId}",
                            () => _depositsRepository.GetNextIdAsync());

                        var deposit = Deposit.Create(
                            outbox.AggregateId,
                            brokerAccountContext.TenantId,
                            tx.BlockchainId,
                            brokerAccountContext.BrokerAccountId,
                            brokerAccountContext.ActiveRequisites.Id,
                            accountContext.Requisites.Id,
                            input,
                            processingContext.TransactionInfo,
                            tx.Sources
                                .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                                .ToArray());

                        processingContext.AddDeposit(deposit);

                        deposits.Add(deposit);
                    }
                }
            }

            var balanceChanges = deposits
                .GroupBy(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                .Select(x => new
                {
                    Id = x.Key,
                    Amount = x.Sum(d => d.Unit.Amount)
                });

            foreach (var change in balanceChanges)
            {
                var balances = processingContext.BrokerAccountBalances[change.Id];

                balances.AddPendingBalance(change.Amount);
            }
        }
    }
}
