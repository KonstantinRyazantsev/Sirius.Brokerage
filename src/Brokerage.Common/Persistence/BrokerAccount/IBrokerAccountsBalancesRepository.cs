using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountsBalancesRepository
    {
        Task<BrokerAccountBalances> GetAsync(long brokerAccountId);
        Task UpdateAsync(BrokerAccountBalances brokerAccountBalances);
        Task<BrokerAccountBalances> AddOrGetAsync(BrokerAccountBalances brokerAccountBalances);
    }
}
