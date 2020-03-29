using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.AccountRequisites;

namespace Brokerage.Common.Persistence
{
    public interface IAccountRequisitesRepository
    {
        Task<IReadOnlyCollection<AccountRequisites>> SearchAsync(
            long accountId,
            int limit,
            long? cursor,
            bool sortAsc);
        Task<AccountRequisites> GetAsync(string brokerAccountRequisitesId);

        Task<AccountRequisites> AddOrGetAsync(AccountRequisites brokerAccount);

        Task UpdateAsync(AccountRequisites brokerAccount);
    }
}
