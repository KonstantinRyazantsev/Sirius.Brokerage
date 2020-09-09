using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Indexer.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class DetectedBrokerDepositProcessor : IDetectedTransactionProcessor
    {
        private readonly IIdGenerator _idGenerator;

        public DetectedBrokerDepositProcessor(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public async Task Process(TransactionDetected tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return;
            }

            var sourceBrokerAccountAddress = processingContext.Operation?.Type == OperationType.Withdrawal
                ? tx.Sources.Select(x => x.Address).Distinct().Single()
                : null;

            var deposits = new List<Deposit>();

            foreach (var brokerAccountContext in processingContext.BrokerAccounts.Where(x => !x.Accounts.Any()))
            {
                foreach (var ((brokerAccountDetailsId, assetId), value) in brokerAccountContext.Income)
                {
                    // Ignores change returned to the broker account on a withdrawal
                    if (processingContext.Operation?.Type == OperationType.Withdrawal &&
                        brokerAccountContext.MatchedBrokerAccountDetails.TryGetValue(brokerAccountDetailsId, out var brokerAccount) 
                            && brokerAccount.NaturalId.Address == sourceBrokerAccountAddress)
                    {
                        continue;
                    }

                    var depositId = await _idGenerator.GetId($"BrokerDeposits:{tx.TransactionId}-{brokerAccountDetailsId}-{assetId}", IdGenerators.Deposits);
                    
                    var deposit = Deposit.Create(
                        depositId,
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
                            .ToArray(),
                        null);

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
