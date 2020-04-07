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

        Task<IReadOnlyCollection<BrokerAccountRequisites>> GetByAddressesAsync(string blockchainId, IReadOnlyCollection<string> addresses);

        Task UpdateAsync(BrokerAccountRequisites brokerAccount);
        Task<BrokerAccountRequisites> GetByIdAsync(long depositBrokerAccountRequisitesId);
    }
}
