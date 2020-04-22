using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountsBalancesRepository
    {
        Task<long> GetNextIdAsync();
        Task<BrokerAccountBalances> GetOrDefaultAsync(BrokerAccountBalancesId id);
        Task SaveAsync(string updatePrefix, IReadOnlyCollection<BrokerAccountBalances> balances);
        Task<IReadOnlyCollection<BrokerAccountBalances>> GetAnyOfAsync(ISet<BrokerAccountBalancesId> ids);
        
    }
}
