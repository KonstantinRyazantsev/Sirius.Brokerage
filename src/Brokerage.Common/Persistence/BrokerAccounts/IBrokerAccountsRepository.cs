using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    public interface IBrokerAccountsRepository
    {
        Task<BrokerAccount> Get(long brokerAccountId);
        Task<BrokerAccount> GetOrDefault(long brokerAccountId);
        Task Add(BrokerAccount brokerAccount);
        Task Update(BrokerAccount brokerAccount);
        Task<IReadOnlyCollection<BrokerAccount>> GetAllOf(IReadOnlyCollection<long> brokerAccountIds);
    }
}
