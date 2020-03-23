using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public interface IBrokerAccountRepository
    {
        Task<BrokerAccount> GetAsync(long brokerAccountId);

        Task<BrokerAccount> AddOrGetAsync(BrokerAccount brokerAccount);

        Task UpdateAsync(BrokerAccount brokerAccount);
    }
}
