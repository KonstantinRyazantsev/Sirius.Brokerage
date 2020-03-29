using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccountRequisites;

namespace Brokerage.Common.Persistence
{
    public interface IBrokerAccountRequisitesRepository
    {
        Task<IReadOnlyCollection<BrokerAccountRequisites>> SearchAsync(long brokerAccountId,
            int limit,
            long? cursor,
            bool sortAsc);

        Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccount);

        Task UpdateAsync(BrokerAccountRequisites brokerAccount);
    }
}
