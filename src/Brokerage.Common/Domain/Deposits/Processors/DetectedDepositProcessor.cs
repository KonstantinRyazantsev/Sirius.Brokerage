using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Configuration;
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
    public class DetectedDepositProcessor : IDetectedTransactionProcessor
    {
        private readonly IIdGenerator _idGenerator;
        private IReadOnlyDictionary<string, BlockchainConfig> _blockchainsConfig;

        public DetectedDepositProcessor(IIdGenerator idGenerator, AppConfig appConfig)
        {
            _idGenerator = idGenerator;
            _blockchainsConfig = appConfig.Blockchains;
        }

        public async Task Process(TransactionDetected tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return;
            }

            var minDepositForConsolidation = 0m;
            if (processingContext.Blockchain.Protocol.Capabilities.DestinationTag == null &&
                _blockchainsConfig.TryGetValue(processingContext.Blockchain.Id, out var blockchainConfiguration))
            {
                minDepositForConsolidation = blockchainConfiguration.MinDepositForConsolidation;
            }

            var deposits = new List<Deposit>();

            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var accountContext in brokerAccountContext.Accounts)
                {
                    foreach (var (assetId, value) in accountContext.Income.Where(x => x.Value > 0))
                    {
                        var depositId = await _idGenerator.GetId($"Deposits:{tx.TransactionId}-{accountContext.Details.Id}-{assetId}", IdGenerators.Deposits);

                        var deposit = value >= minDepositForConsolidation ? Deposit.Create(
                            depositId,
                            brokerAccountContext.TenantId,
                            tx.BlockchainId,
                            brokerAccountContext.BrokerAccountId,
                            brokerAccountContext.ActiveDetails.Id,
                            accountContext.Details.Id,
                            new Unit(assetId, value),
                            processingContext.TransactionInfo,
                            tx.Sources
                                .Where(x => x.Unit.AssetId == assetId)
                                .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                                .ToArray()) :
                                Deposit.CreateMin(
                                    depositId,
                                    brokerAccountContext.TenantId,
                                    tx.BlockchainId,
                                    brokerAccountContext.BrokerAccountId,
                                    brokerAccountContext.ActiveDetails.Id,
                                    accountContext.Details.Id,
                                    new Unit(assetId, value),
                                    processingContext.TransactionInfo,
                                    tx.Sources
                                        .Where(x => x.Unit.AssetId == assetId)
                                        .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                                        .ToArray())
                            ;

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
