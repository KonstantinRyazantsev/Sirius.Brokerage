using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.BrokerAccount;

namespace BrokerageTests.UnitTests
{
    public class InMemoryBrokerAccountsBalancesRepository : IBrokerAccountsBalancesRepository
    {
        private long id = 0;
        private readonly List<BrokerAccountBalances> _storage;

        public InMemoryBrokerAccountsBalancesRepository()
        {
            _storage = new List<BrokerAccountBalances>();
        }

        public Task<BrokerAccountBalances> GetOrDefaultAsync(long brokerAccountId, long assetId)
        {
            return Task.FromResult(_storage
                .FirstOrDefault(x => x.BrokerAccountId == brokerAccountId &&
                                                                x.AssetId == assetId));
        }

        public Task SaveAsync(BrokerAccountBalances brokerAccountBalances, string updateId)
        {
            id++;

            _storage.Add(BrokerAccountBalances.Restore(
                id,
                brokerAccountBalances.Sequence++,
                brokerAccountBalances.Version,
                brokerAccountBalances.BrokerAccountId,
                brokerAccountBalances.AssetId,
                brokerAccountBalances.OwnedBalance,
                brokerAccountBalances.AvailableBalance,
                brokerAccountBalances.PendingBalance,
                brokerAccountBalances.ReservedBalance,
                brokerAccountBalances.OwnedBalanceUpdateDateTime,
                brokerAccountBalances.AvailableBalanceUpdateDateTime,
                brokerAccountBalances.PendingBalanceUpdateDateTime,
                brokerAccountBalances.ReservedBalanceUpdateDateTime));

            return Task.CompletedTask;
        }
    }
}
