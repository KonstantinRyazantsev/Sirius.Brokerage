using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountRequisitesRepository
    {
        Task<long> GetNextIdAsync();
        Task<IReadOnlyCollection<BrokerAccountRequisites>> GetByBrokerAccountAsync(long brokerAccountId,
            int limit,
            long? cursor);
        Task<IReadOnlyCollection<BrokerAccountRequisites>> GetAnyOfAsync(ISet<BrokerAccountRequisitesId> ids);
        Task AddOrIgnoreAsync(BrokerAccountRequisites brokerAccount);
        Task<BrokerAccountRequisites> GetAsync(long id);
        Task<IReadOnlyDictionary<ActiveBrokerAccountRequisitesId, BrokerAccountRequisites>> GetActiveAsync(ISet<ActiveBrokerAccountRequisitesId> ids);
        Task<BrokerAccountRequisites> GetActiveAsync(ActiveBrokerAccountRequisitesId id);
    }
}
