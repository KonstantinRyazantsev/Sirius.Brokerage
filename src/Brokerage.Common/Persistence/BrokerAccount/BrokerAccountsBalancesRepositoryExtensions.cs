using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public static class BrokerAccountsBalancesRepositoryExtensions
    {
        public static async Task<BrokerAccountBalances> GetAsync(this IBrokerAccountsBalancesRepository repo, BrokerAccountBalancesId id)
        {
            var balances = await repo.GetOrDefaultAsync(id);

            if (balances == null)
            {
                throw new InvalidOperationException($"Broker account balances {id} not found");
            }

            return balances;
        }
    }
}
