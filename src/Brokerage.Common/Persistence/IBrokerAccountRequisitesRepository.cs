using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccountRequisites;

namespace Brokerage.Common.Persistence
{
    public interface IBrokerAccountRequisitesRepository
    {
        Task<BrokerAccountRequisites> GetAsync(string brokerAccountRequisitesId);

        Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccount);

        Task UpdateAsync(BrokerAccountRequisites brokerAccount);
    }
}
