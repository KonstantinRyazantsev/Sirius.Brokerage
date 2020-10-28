using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits.Implementations;
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

        public DetectedDepositProcessor(
            IIdGenerator idGenerator,
            AppConfig appConfig)
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

            var minDepositForConsolidation = TinyDepositsAmountExtractor.GetMinDepositForConsolidation(processingContext.Blockchain,
                _blockchainsConfig);

            var deposits = new List<Deposit>();

            var blockchain = processingContext.Blockchain;

            foreach (var brokerAccountContext in processingContext.BrokerAccounts)
            {
                foreach (var accountContext in brokerAccountContext.Accounts)
                {
                    foreach (var (assetId, value) in accountContext.Income.Where(x => x.Value > 0))
                    {
                        DepositType depositType;
                        if (blockchain.Protocol.FeePayingSiriusAssetId == assetId)
                            depositType = value >= minDepositForConsolidation ? DepositType.Regular : DepositType.Tiny;
                        else
                            depositType = value >= minDepositForConsolidation ? DepositType.Token : DepositType.TinyToken;

                        var depositId = await _idGenerator.GetId(
                            $"Deposits:{tx.TransactionId}-{accountContext.Details.Id}-{assetId}",
                            IdGenerators.Deposits);

                        var transferSources = tx.Sources
                            .Where(x => x.Unit.AssetId == assetId)
                            .Select(x => new DepositSource(x.Address, x.Unit.Amount))
                            .ToArray();

                        switch (depositType)
                        {
                            case DepositType.Regular:
                                {
                                    var deposit = RegularDeposit.Create(
                                        depositId,
                                        brokerAccountContext.TenantId,
                                        tx.BlockchainId,
                                        brokerAccountContext.BrokerAccountId,
                                        brokerAccountContext.ActiveDetails.Id,
                                        accountContext.Details.Id,
                                        new Unit(assetId, value),
                                        processingContext.TransactionInfo,
                                        transferSources,
                                        minDepositForConsolidation);

                                    processingContext.AddDeposit(deposit);

                                    deposits.Add(deposit);
                                }
                                break;
                            case DepositType.Tiny:
                                {
                                    var deposit = TinyDeposit.Create(
                                        depositId,
                                        brokerAccountContext.TenantId,
                                        tx.BlockchainId,
                                        brokerAccountContext.BrokerAccountId,
                                        brokerAccountContext.ActiveDetails.Id,
                                        accountContext.Details.Id,
                                        new Unit(assetId, value),
                                        processingContext.TransactionInfo,
                                        transferSources,
                                        minDepositForConsolidation);

                                    processingContext.AddDeposit(deposit);

                                    deposits.Add(deposit);
                                }
                                break;
                            case DepositType.TinyToken:
                                {
                                    var deposit = TinyTokenDeposit.Create(
                                        depositId,
                                        brokerAccountContext.TenantId,
                                        tx.BlockchainId,
                                        brokerAccountContext.BrokerAccountId,
                                        brokerAccountContext.ActiveDetails.Id,
                                        accountContext.Details.Id,
                                        new Unit(assetId, value),
                                        processingContext.TransactionInfo,
                                        transferSources,
                                        minDepositForConsolidation);

                                    processingContext.AddDeposit(deposit);

                                    deposits.Add(deposit);
                                }
                                break;

                            case DepositType.Token:
                                {
                                    var deposit = TokenDeposit.Create(
                                        depositId,
                                        brokerAccountContext.TenantId,
                                        tx.BlockchainId,
                                        brokerAccountContext.BrokerAccountId,
                                        brokerAccountContext.ActiveDetails.Id,
                                        accountContext.Details.Id,
                                        new Unit(assetId, value),
                                        processingContext.TransactionInfo,
                                        transferSources,
                                        minDepositForConsolidation);

                                    processingContext.AddDeposit(deposit);

                                    deposits.Add(deposit);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(depositType), depositType, null);
                        }
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
