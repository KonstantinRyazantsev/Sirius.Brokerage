using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    public interface IBrokerAccountsBalancesRepository
    {
        Task<BrokerAccountBalances> Get(BrokerAccountBalancesId id);
        Task<BrokerAccountBalances> GetOrDefault(BrokerAccountBalancesId id);
        Task Save(IReadOnlyCollection<BrokerAccountBalances> balances);
        Task<IReadOnlyCollection<BrokerAccountBalances>> GetAnyOf(ISet<BrokerAccountBalancesId> ids);
    }
}
