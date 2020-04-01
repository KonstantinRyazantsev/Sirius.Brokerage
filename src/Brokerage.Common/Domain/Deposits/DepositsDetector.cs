using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
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

        public DepositsDetector(
            IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IPublishEndpoint publishEndpoint)
        {
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _publishEndpoint = publishEndpoint;
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

            // TODO: We need transactions batch here to improve DB performance  
            var incomingTransferAddresses = incomingTransfers.Keys
                .Select(x => x.Address)
                .Distinct()
                .ToArray();

            var accountRequisites = await _accountRequisitesRepository.GetByAddressesAsync(
                transaction.BlockchainId,
                incomingTransferAddresses);

            var brokerAccountRequisites = await _brokerAccountRequisitesRepository.GetByAddressesAsync(
                transaction.BlockchainId,
                incomingTransferAddresses);

            var incomingTransfersByAddress = incomingTransfers
                .ToLookup(x => x.Key.Address);

            var transferDict = new Dictionary<(long BrokerAccountId, long AssetId), decimal>();
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
                    }
                }
            }


            foreach (var ((brokerAccountId, assetId), pendingBalanceChange) in transferDict)
            {
                var balances = await _brokerAccountsBalancesRepository.GetOrDefaultAsync(brokerAccountId, assetId);

                if (balances == default)
                {
                    balances = BrokerAccountBalances.Create(brokerAccountId, assetId);
                }

                // Balance:
                //  balance change:
                //      id (tx.Id + broker account balances ID)
                //      broker account balances ID
                //      version
                //      balance type (pending, owned, available, reserved)
                //      broker account requisites id
                //      amount
                //      date time
                
                // Option1: events sourcing

                // Events:
                //string changeId
                //long id,
                //long version,
                //long brokerAccountId,
                //long assetId,
                //decimal ownedBalanceChange,
                //decimal availableBalanceChange,
                //decimal pendingBalanceChange,
                //decimal reservedBalanceChange,
                //DateTime ownedBalanceUpdateDateTime,
                //DateTime availableBalanceUpdateDateTime,
                //DateTime pendingBalanceUpdateDateTime,
                //DateTime reservedBalanceUpdateDateTime

                // Snapshot:
                //long id,
                //long version,
                //long brokerAccountId,
                //long assetId,
                //decimal ownedBalance,
                //decimal availableBalance,
                //decimal pendingBalance,
                //decimal reservedBalance,
                //DateTime ownedBalanceUpdateDateTime,
                //DateTime availableBalanceUpdateDateTime,
                //DateTime pendingBalanceUpdateDateTime,
                //DateTime reservedBalanceUpdateDateTime

                // Option 2: transactions
                
                // Snapshot:
                //long id,
                //long version,
                //long brokerAccountId,
                //long assetId,
                //decimal ownedBalance,
                //decimal availableBalance,
                //decimal pendingBalance,
                //decimal reservedBalance,
                //DateTime ownedBalanceUpdateDateTime,
                //DateTime availableBalanceUpdateDateTime,
                //DateTime pendingBalanceUpdateDateTime,
                //DateTime reservedBalanceUpdateDateTime

                // Updates:
                // string updateId (unique) (broker account balances ID + transaction ID)
                // 
                
                // Update of the snapshot and insert of the change are executed in the single DB transaction
                // if change is already in the DB, then tx will be rolled back and no changes will be applied
                // to the snapshot.

                balances.AddPendingBalance(pendingBalanceChange);
                string updateId = $"{brokerAccountId}_{assetId}_{transaction.TransactionId}";
                await _brokerAccountsBalancesRepository.SaveAsync(balances, updateId);
                foreach (var evt in balances.Events)
                {
                    await _publishEndpoint.Publish(evt);
                }
            }
        }
    }
}
