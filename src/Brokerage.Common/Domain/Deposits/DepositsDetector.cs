﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Entities;
using Brokerage.Common.Persistence.Entities.Deposits;
using MassTransit;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Common.Domain.Deposits
{
    public class DepositsDetector
    {
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IDepositsRepository _depositsRepository;

        public DepositsDetector(
            IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IPublishEndpoint publishEndpoint,
            IDepositsRepository depositsRepository)
        {
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _publishEndpoint = publishEndpoint;
            _depositsRepository = depositsRepository;
        }

        public async Task Detect(TransactionDetected transaction)
        {
            var incomingTransfers = transaction
                .BalanceUpdates
                .SelectMany(x =>
                    x.Transfers
                        .Where(x => x.Amount > 0)
                        .Select(t => new
                        {
                            Address = x.Address,
                            AssetId = x.AssetId,
                            Amount = t.Amount
                        }))
                .GroupBy(x => new
                {
                    x.Address,
                    x.AssetId
                })
                .Select(g => new
                {
                    Address = g.Key.Address,
                    AssetId = g.Key.AssetId,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToDictionary(x => new
                {
                    x.Address,
                    x.AssetId
                });

            var outgoingTransfers = transaction
                .BalanceUpdates
                .SelectMany(x =>
                    x.Transfers
                        .Where(x => x.Amount < 0)
                        .Select(t => new
                        {
                            Address = x.Address,
                            AssetId = x.AssetId,
                            Amount = t.Amount
                        }))
                .ToLookup(x => x.AssetId);

            // TODO: We need transactions batch here to improve DB performance  
            var incomingTransferAddresses = incomingTransfers.Keys
                .Select(x => x.Address)
                .Distinct()
                .ToArray();

            var accountRequisites = await _accountRequisitesRepository.GetByAddressesAsync(
                transaction.BlockchainId,
                incomingTransferAddresses);

            var accountRequisitesByAddressDict = accountRequisites
                .ToDictionary(x => x.Address);

            var brokerAccountRequisites = await _brokerAccountRequisitesRepository.GetByAddressesAsync(
                transaction.BlockchainId,
                incomingTransferAddresses);

            var brokerAccountRequisitesByBrokerAccountIdDict = brokerAccountRequisites
                .ToDictionary(x => x.BrokerAccountId);

            var incomingTransfersByAddress = incomingTransfers
                .ToLookup(x => x.Key.Address);

            var transferDict = new Dictionary<(long BrokerAccountId, long AssetId), decimal>();
            var depositsDict = new Dictionary<(long BrokerAccountId, long AssetId, string address), decimal>();
            var brokerAccountsAddressAmounts = accountRequisites
                .Select(x => new
                {
                    x.BrokerAccountId,
                    x.Address,
                })
                .Union(brokerAccountRequisites.Select(x => new
                {
                    x.BrokerAccountId,
                    x.Address
                }))
                .Select(x => new
                {
                    x.Address,
                    x.BrokerAccountId,
                    AmountByAsset = incomingTransfersByAddress[x.Address]
                        .ToDictionary(
                            t => t.Key.AssetId,
                            t => t.Value.Amount)
                })
                .GroupBy(x => x.BrokerAccountId);

            foreach (var brokerAccountsAddressAmount in brokerAccountsAddressAmounts)
            {
                var brokerAccountId = brokerAccountsAddressAmount.Key;
                foreach (var addressAmount in brokerAccountsAddressAmount)
                {
                    foreach (var (assetId, amount) in addressAmount.AmountByAsset)
                    {
                        transferDict.TryGetValue((brokerAccountId, assetId), out var existing);
                        transferDict[(brokerAccountId, assetId)] = existing + amount;

                        depositsDict.TryGetValue((brokerAccountId, assetId, addressAmount.Address), out var existingDeposit);
                        depositsDict[(brokerAccountId, assetId, addressAmount.Address)] = existingDeposit + amount;
                    }
                }
            }


            foreach (var ((brokerAccountId, assetId), pendingBalanceChange) in transferDict)
            {
                var balances = await _brokerAccountsBalancesRepository.GetOrDefaultAsync(brokerAccountId, assetId);

                if (balances == default)
                {
                    var id = await _brokerAccountsBalancesRepository.GetNextIdAsync();

                    balances = BrokerAccountBalances.Create(id, brokerAccountId, assetId);
                }

                balances.AddPendingBalance(pendingBalanceChange);

                var updateId = $"{brokerAccountId}_{assetId}_{transaction.TransactionId}_{TransactionStage.Detected}";
                //Optimistic concurrency excpetion (Update failed - ignore), Add failed

                //try
                //{
                await _brokerAccountsBalancesRepository.SaveAsync(balances, updateId);

                foreach (var evt in balances.Events)
                {
                    await _publishEndpoint.Publish(evt);
                }
                //}
                //catch (OptimisticConcurrencyException e) when (e.Reason == OptimisticConcurrencyReason.UpdateFailed)
                //{
                //}
            }

            foreach (var ((brokerAccountId, assetId, address), depositBalance) in depositsDict)
            {
                var requisites = brokerAccountRequisitesByBrokerAccountIdDict[brokerAccountId];
                accountRequisitesByAddressDict.TryGetValue(address, out var accountRequisitesVal);

                var transactionInfo = new TransactionInfo(
                    transaction.TransactionId,
                    transaction.BlockNumber,
                    // TODO: Take from somewhere
                    1,
                    DateTime.UtcNow);

                var sources = outgoingTransfers[assetId]
                    .Select(x => new DepositSource(x.Address, x.Amount))
                    .ToArray();

                var id = await _depositsRepository.GetNextIdAsync();

                var deposit = Deposit.Create(id,
                    requisites.Id,
                    accountRequisitesVal?.AccountRequisitesId,
                    assetId,
                    depositBalance,
                    new DepositFee[0],
                    transactionInfo,
                    null,
                    sources,
                    address);

                try
                {
                    await _depositsRepository.SaveAsync(deposit);

                    foreach (var evt in deposit.Events)
                    {
                        await _publishEndpoint.Publish(evt);
                    }
                }
                //Catch optimistic concurrency exception
                catch (Exception e)
                {
                    //Get
                    deposit = await _depositsRepository.GetOrDefaultAsync(
                        transaction.TransactionId,
                        assetId,
                        requisites.Id,
                        accountRequisitesVal?.AccountRequisitesId);

                }
            }
        }
    }
}
