using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccount;

namespace Brokerage.Common.Persistence
{
    public interface IBrokerAccountRepository
    {
        Task<BrokerAccount> GetAsync(long brokerAccountId);

        Task<BrokerAccount> AddOrGetAsync(
            string requestId,
            string tenantId,
            string blockchainId, 
            string networkId,
            string name);
    }
}
