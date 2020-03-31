using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountRequisitesRepository
    {
        Task<IReadOnlyCollection<BrokerAccountRequisites>> SearchAsync(long? brokerAccountId,
            int limit,
            long? cursor,
            bool sortAsc,
            string blockchainId,
            string address);

        Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccount);

        Task UpdateAsync(BrokerAccountRequisites brokerAccount);
    }
}
