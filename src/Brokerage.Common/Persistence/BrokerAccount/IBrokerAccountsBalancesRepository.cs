using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountsBalancesRepository
    {
        Task<BrokerAccountBalances> GetOrDefaultAsync(long brokerAccountId, long assetId);
        Task SaveAsync(BrokerAccountBalances brokerAccountBalances);
    }
}
