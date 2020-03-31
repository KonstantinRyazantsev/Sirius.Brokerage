using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Common.Domain.Deposits
{
    public class DepositsDetector
    {
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;

        public DepositsDetector(IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository)
        {
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
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
                .GroupBy(x => x.BrokerAccountId)
                .Select(g => new
                {
                    BrokerAccountId = g.Key,
                    AmountByAsset = g.ToDictionary()
                })

            foreach (var brokerAccountAddressAmounts in brokerAccountsAddressAmounts)
            {
                foreach (var incomingAsset in brokerAccountAddressAmounts.)
                {
                    var addressAsset = new
                    {
                        Address = brokerAccountAddress.Address,
                        AssetId = incomingAsset.Key.AssetId
                    };
                    var pendingBalanceChange = incomingTransfers[addressAsset].Amount;

                    var balances = await _brokerAccountsBalancesRepository.AddOrGetAsync(BrokerAccountBalances.Create(
                        brokerAccountAddress.BrokerAccountId,
                        addressAsset.AssetId));

                    balances.AddPendingBalance(pendingBalanceChange);

                    await _brokerAccountsBalancesRepository.UpdateAsync(balances);
                }
            }
        }
    }
}
