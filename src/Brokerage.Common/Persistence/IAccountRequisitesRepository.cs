using System.Threading.Tasks;
using Brokerage.Common.Domain.AccountRequisites;

namespace Brokerage.Common.Persistence
{
    public interface IAccountRequisitesRepository
    {
        Task<AccountRequisites> GetAsync(string brokerAccountRequisitesId);

        Task<AccountRequisites> AddOrGetAsync(AccountRequisites brokerAccount);

        Task UpdateAsync(AccountRequisites brokerAccount);
    }
}
