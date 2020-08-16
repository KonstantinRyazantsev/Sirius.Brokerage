using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    public interface IBrokerAccountDetailsRepository
    {
        Task<IReadOnlyCollection<BrokerAccountDetails>> GetAnyOf(ISet<BrokerAccountDetailsId> ids);
        Task Add(BrokerAccountDetails brokerAccount);
        Task<BrokerAccountDetails> Get(long id);
        Task<IReadOnlyDictionary<ActiveBrokerAccountDetailsId, BrokerAccountDetails>> GetActive(ISet<ActiveBrokerAccountDetailsId> ids);
        Task<BrokerAccountDetails> GetActive(ActiveBrokerAccountDetailsId id);
        Task<long> GetCountByBrokerAccountId(long brokerAccountId);
    }
}
