using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountDetailsRepository
    {
        Task<long> GetNextIdAsync();
        Task<IReadOnlyCollection<BrokerAccountDetails>> GetByBrokerAccountAsync(long brokerAccountId,
            int limit,
            long? cursor);
        Task<IReadOnlyCollection<BrokerAccountDetails>> GetAnyOfAsync(ISet<BrokerAccountDetailsId> ids);
        Task AddOrIgnoreAsync(BrokerAccountDetails brokerAccount);
        Task<BrokerAccountDetails> GetAsync(long id);
        Task<IReadOnlyDictionary<ActiveBrokerAccountDetailsId, BrokerAccountDetails>> GetActiveAsync(ISet<ActiveBrokerAccountDetailsId> ids);
        Task<BrokerAccountDetails> GetActiveAsync(ActiveBrokerAccountDetailsId id);
    }
}
