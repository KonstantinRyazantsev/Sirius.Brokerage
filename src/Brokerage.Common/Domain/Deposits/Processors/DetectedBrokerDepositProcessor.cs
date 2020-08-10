using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Deposits;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Indexer.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class DetectedBrokerDepositProcessor : IDetectedTransactionProcessor
    {
        private readonly IOutboxManager _outboxManager;
        private readonly IDepositsRepository _depositsRepository;

        public DetectedBrokerDepositProcessor(IOutboxManager outboxManager, 
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

            var sourceBrokerAccountAddress = processingContext.Operation?.Type == OperationType.Withdrawal
                ? tx.Sources.Select(x => x.Address).Single()
                : null;

            var deposits = new List<Deposit>();

            foreach (var brokerAccountContext in processingContext.BrokerAccounts.Where(x => !x.Accounts.Any()))
            {
                foreach (var ((brokerAccountDetailsId, assetId), value) in brokerAccountContext.Income)
                {
                    if (processingContext.Operation?.Type == OperationType.Withdrawal &&
                        brokerAccountContext.BrokerAccountDetails.TryGetValue(brokerAccountDetailsId, out var brokerAccount) 
                            && brokerAccount.NaturalId.Address == sourceBrokerAccountAddress)
                    {
                        continue;
                    }

                    var outbox = await _outboxManager.Open(
                        $"BrokerDeposits:Create:{tx.TransactionId}-{brokerAccountDetailsId}-{assetId}",
                        () => _depositsRepository.GetNextIdAsync());

                    var deposit = Deposit.Create(
                        outbox.AggregateId,
                        brokerAccountContext.TenantId,
                        tx.BlockchainId,
                        brokerAccountContext.BrokerAccountId,
                        brokerAccountDetailsId,
                        null,
                        new Unit(assetId, value), 
                        processingContext.TransactionInfo,
                        tx.Sources
                            .Where(x => x.Unit.AssetId == assetId)
                            .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                            .ToArray());

                    processingContext.AddDeposit(deposit);

                    deposits.Add(deposit);
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
