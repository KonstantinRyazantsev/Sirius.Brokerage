using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Entities;
using MassTransit;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits
{
    public class DepositsConfirmator
    {
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public DepositsConfirmator(
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

        public async Task Confirm(TransactionConfirmed transaction)
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

            var brokerAccountRequisitesHashSet = brokerAccountRequisites
                .Select(x => x.Address)
                .ToHashSet();

            var incomingTransfersByAddress = incomingTransfers
                .ToLookup(x => x.Key.Address);

            var transferDict = new Dictionary<(long BrokerAccountId, long AssetId), AddressAmount>();
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
                        if (!transferDict.TryGetValue((brokerAccountId, assetId), out var existing))
                        {
                            transferDict[(brokerAccountId, assetId)] = new AddressAmount();
                            existing = transferDict[(brokerAccountId, assetId)];
                        }

                        //Deposit to depositWallet
                        if (!brokerAccountRequisitesHashSet.Contains(addressAmount.Address))
                        {
                            existing.OwnedAmount += amount;
                        }
                        //Deposit to hotWallet
                        else
                        {
                            existing.OwnedAmount += amount;
                            existing.AvailabeAmount += amount;
                        }
                    }
                }
            }

            foreach (var ((brokerAccountId, assetId), balanceChange) in transferDict)
            {
                var balances = await _brokerAccountsBalancesRepository.GetOrDefaultAsync(brokerAccountId, assetId);

                if (balances == default)
                {
                    var id = await _brokerAccountsBalancesRepository.GetNextIdAsync();

                    balances = BrokerAccountBalances.Create(id, brokerAccountId, assetId);
                }

                balances.MovePendingBalanceToAvailableAndOwned(balanceChange.OwnedAmount, balanceChange.AvailabeAmount);

                var updateId = $"{brokerAccountId}_{assetId}_{transaction.TransactionId}_{TransactionStage.Confirmed}";
                await _brokerAccountsBalancesRepository.SaveAsync(balances, updateId);

                foreach (var evt in balances.Events)
                {
                    await _publishEndpoint.Publish(evt);
                }
            }
        }
    }

    internal class AddressAmount
    {
        public string Address { get; set; }

        public decimal AvailabeAmount { get; set; }

        public decimal OwnedAmount { get; set; }
    }
}
