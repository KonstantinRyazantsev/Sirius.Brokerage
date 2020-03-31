using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence
{
    public interface IAccountRequisitesRepository
    {
        Task<IReadOnlyCollection<AccountRequisites>> GetByAccountAsync(
            long accountId,
            int limit,
            long? cursor,
            bool sortAsc);

        Task<AccountRequisites> AddOrGetAsync(AccountRequisites requisites);

        Task UpdateAsync(AccountRequisites requisites);
        
        Task<IReadOnlyCollection<AccountRequisites>> GetAllAsync(long? cursor, int limit);
    }
}
