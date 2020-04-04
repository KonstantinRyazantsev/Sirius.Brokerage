using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.BrokerAccount;

namespace BrokerageTests.Repositories
{
    public class InMemoryBrokerAccountsBalancesRepository : IBrokerAccountsBalancesRepository
    {
        private long _id = 0;
        private readonly List<BrokerAccountBalances> _storage;

        public InMemoryBrokerAccountsBalancesRepository()
        {
            _storage = new List<BrokerAccountBalances>();
        }

        public Task<long> GetNextIdAsync()
        {
            return Task.FromResult(Interlocked.Increment(ref _id));
        }

        public Task<BrokerAccountBalances> GetOrDefaultAsync(long brokerAccountId, long assetId)
        {
            return Task.FromResult(_storage
                .FirstOrDefault(x => x.BrokerAccountId == brokerAccountId &&
                                                                x.AssetId == assetId));
        }

        public Task SaveAsync(BrokerAccountBalances brokerAccountBalances, string updateId)
        {
            _storage.Add(BrokerAccountBalances.Restore(
                brokerAccountBalances.Id,
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
