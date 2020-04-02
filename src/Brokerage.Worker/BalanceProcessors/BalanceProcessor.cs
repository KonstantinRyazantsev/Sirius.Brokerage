using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;
using Brokerage.Bilv1.Domain.Models.Operations;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Indexer.MessagingContract;
using BalanceUpdate = Swisschain.Sirius.Indexer.MessagingContract.BalanceUpdate;
using Fee = Swisschain.Sirius.Indexer.MessagingContract.Fee;
using Transfer = Swisschain.Sirius.Indexer.MessagingContract.Transfer;

namespace Brokerage.Worker.BalanceProcessors
{
    public class BalanceProcessor
    {
        private readonly string _blockchainId;
        private readonly ILogger<BalanceProcessor> _logger;
        private readonly IBlockchainApiClient _blockchainApiClient;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;

        private readonly IReadOnlyDictionary<string, (string BlockchainId, long BlockchainAssetId)> _assets =
            new ReadOnlyDictionary<string, (string BlockchainId, long BlockchainAssetId)>(
                new Dictionary<string, (string BlockchainId, long BlockchainAssetId)>()
                {
                    {"bitcoin-regtest", ("bitcoin-regtest", 100_000)},
                    {"ethereum-ropsten", ("ethereum-ropsten", 100_001)}
                });

        private IReadOnlyDictionary<string, BlockchainAsset> _blockchainAssets;
        private readonly IOperationRepository _operationRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly DepositsDetector _depositsDetector;
        private readonly DepositsConfirmator _depositsConfirmator;

        public BalanceProcessor(string blockchainId,
            ILogger<BalanceProcessor> logger,
            IBlockchainApiClient blockchainApiClient,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IOperationRepository operationRepository,
            IPublishEndpoint eventPublisher,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository,
            DepositsDetector depositsDetector,
            DepositsConfirmator depositsConfirmator,
            IReadOnlyDictionary<string, BlockchainAsset> blockchainAssets)
        {
            _blockchainId = blockchainId;
            _logger = logger;
            _blockchainApiClient = blockchainApiClient;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _blockchainAssets = blockchainAssets;
            _operationRepository = operationRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _accountRequisitesRepository = accountRequisitesRepository;
            _depositsDetector = depositsDetector;
            _depositsConfirmator = depositsConfirmator;
        }

        public async Task ProcessAsync(int batchSize)
        {
            var skip = 0;
            var balancesFromDatabase = new List<EnrolledBalance>();
            do
            {
                var received = await _enrolledBalanceRepository.GetAllForBlockchainAsync(_blockchainId, skip, 100);

                if (received == null || !received.Any())
                    break;

                balancesFromDatabase.AddRange(received);

                skip += received.Count;

                if (received.Count < 100)
                    break;

            } while (true);

            var balancesFromApi = new Dictionary<(string AssetId, string Address), WalletBalance>();
            await _blockchainApiClient.EnumerateWalletBalanceBatchesAsync(
                batchSize,
                assetId => GetAssetAccuracy(assetId, batchSize),
                batch =>
                {
                    if (batch != null && batch.Any())
                    {
                        foreach (var item in batch)
                        {
                            balancesFromApi[(item.AssetId, item.Address)] = item;
                        }
                    }

                    return Task.FromResult(true);
                });

            await ProcessBalancesBatchAsync(balancesFromApi, balancesFromDatabase);
        }

        private async Task ProcessBalancesBatchAsync(
            IDictionary<(string AssetId, string Address), WalletBalance> fromApi,
            List<EnrolledBalance> fromDatabase)
        {
            foreach (var balance in fromDatabase)
            {
                await ProcessBalance(balance, fromApi);
            }
        }

        private async Task ProcessBalance(EnrolledBalance enrolledBalance,
            IDictionary<(string AssetId, string Address), WalletBalance> depositWallets)
        {
            var key = enrolledBalance.Key;

            if (!_blockchainAssets.TryGetValue(key.BlockchainAssetId, out var blockchainAsset))
            {
                _logger.LogWarning("Blockchain asset is not found {depositWallet}}", enrolledBalance);

                return;
            }

            depositWallets.TryGetValue(
                (enrolledBalance.Key.BlockchainAssetId,
                    enrolledBalance.Key.WalletAddress),
                out var depositWallet);


            var balance = depositWallet?.Balance ?? 0m;
            var balanceBlock = depositWallet?.Block ?? enrolledBalance.Block;

            var cashinCouldBeStarted = CouldBeStarted(
                balance,
                balanceBlock,
                enrolledBalance.Balance,
                enrolledBalance.Block,
                out var operationAmount);

            if (!cashinCouldBeStarted)
            {
                return;
            }

            _assets.TryGetValue(_blockchainId, out var blockchainMapped);

            var operation = await _operationRepository.AddAsync(key, operationAmount, enrolledBalance.Block);

            var detectedTransaction = new TransactionDetected()
            {
                BlockchainId = blockchainMapped.BlockchainId,
                BalanceUpdates = new BalanceUpdate[]
                {
                    new BalanceUpdate()
                    {
                        Address = key.WalletAddress,
                        AssetId = blockchainMapped.BlockchainAssetId,
                        Transfers = new List<Transfer>()
                        {
                            new Transfer()
                            {
                                Amount = operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                },
                BlockId = "some block",
                BlockNumber = balanceBlock,
                ErrorCode = null,
                ErrorMessage = null,
                Fees = new Fee[0],
                TransactionId = operation.OperationId.ToString(),
                TransactionNumber = 0,
            };

            var confirmedTransaction = new TransactionConfirmed()
            {
                BlockchainId = blockchainMapped.BlockchainId,
                BalanceUpdates = new Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate[]
                {
                    new Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate()
                    {
                        Address = key.WalletAddress,
                        AssetId = blockchainMapped.BlockchainAssetId,
                        Transfers = new List<Swisschain.Sirius.Confirmator.MessagingContract.Transfer>()
                        {
                            new Swisschain.Sirius.Confirmator.MessagingContract.Transfer()
                            {
                                Amount = operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                },
                BlockId = "some block",
                BlockNumber = balanceBlock,
                ErrorCode = null,
                Fees = new Swisschain.Sirius.Confirmator.MessagingContract.Fee[0],
                TransactionId = operation.OperationId.ToString(),
                TransactionNumber = 0,
            };

            if (operationAmount > 0)
            {
                await _depositsDetector.Detect(detectedTransaction);

                await _depositsConfirmator.Confirm(confirmedTransaction);
            }
            else
            {
                // TODO: Withdrawal
            }

            await _enrolledBalanceRepository.SetBalanceAsync(
                key,
                balance,
                balanceBlock);
        }


        private static bool CouldBeStarted(
            decimal balanceAmount,
            BigInteger balanceBlock,
            decimal enrolledBalanceAmount,
            BigInteger enrolledBalanceBlock,
            out decimal operationAmount)
        {
            operationAmount = 0;

            if (balanceBlock < enrolledBalanceBlock)
            {
                // This balance was already processed
                return false;
            }

            operationAmount = balanceAmount - enrolledBalanceAmount;

            if (operationAmount == 0)
            {
                // No visbible changes have happened since the last check
                return false;
            }

            return true;
        }

        private async Task<IReadOnlyDictionary<string, EnrolledBalance>> GetEnrolledBalancesAsync(IEnumerable<WalletBalance>
        balances)
        {
            var walletKeys = balances.Select(x => new DepositWalletKey(
                    blockchainAssetId: x.AssetId,
                    blockchainId: _blockchainId,
                    depositWalletAddress: x.Address
                ));

            return (await _enrolledBalanceRepository.GetAsync(walletKeys))
            .ToDictionary(x => GetEnrolledBalancesDictionaryKey(x.Key.WalletAddress, x.Key.BlockchainAssetId),
        y => y);
        }

        private int GetAssetAccuracy(string assetId, int batchSize)
        {
            if (!_blockchainAssets.TryGetValue(assetId, out var asset))
            {
                // Unknown asset, tries to refresh cached assets

                _blockchainAssets = _blockchainApiClient
                    .GetAllAssetsAsync(batchSize)
                    .GetAwaiter()
                    .GetResult();

                if (!_blockchainAssets.TryGetValue(assetId, out asset))
                {
                    throw new InvalidOperationException($"Asset {assetId} not found");
                }
            }

            return asset.Accuracy;
        }

        private string GetEnrolledBalancesDictionaryKey(string address, string assetId)
        {
            return $"{address.ToLower(CultureInfo.InvariantCulture)}:{assetId}";
        }
    }

}

