using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountRequisitesRepository
    {
        Task<IReadOnlyCollection<BrokerAccountRequisites>> GetAllAsync(long? brokerAccountId,
            int limit,
            long? cursor,
            bool sortAsc,
            string blockchainId,
            string address);
        Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccount);
        Task<IReadOnlyCollection<BrokerAccountRequisites>> GetAnyOfAsync(ISet<BrokerAccountRequisitesId> ids);
        Task UpdateAsync(BrokerAccountRequisites brokerAccount);
        Task<BrokerAccountRequisites> GetByIdAsync(long brokerAccountRequisitesId);
        Task<BrokerAccountRequisites> GetActualByBrokerAccountIdAndBlockchainAsync(long brokerAccountId, string blockchainId);
    }
}
