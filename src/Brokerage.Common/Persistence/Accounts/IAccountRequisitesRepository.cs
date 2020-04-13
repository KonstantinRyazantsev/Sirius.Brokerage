using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public interface IAccountRequisitesRepository
    {
        Task AddOrIgnoreAsync(AccountRequisites requisites);
        Task<IReadOnlyCollection<AccountRequisites>> GetByAccountAsync(long accountId,
            int limit,
            long? cursor,
            bool sortAsc);
        Task<IReadOnlyCollection<AccountRequisites>> GetAnyOfAsync(ISet<AccountRequisitesId> ids);
        Task<IReadOnlyCollection<AccountRequisites>> GetAllAsync(long? cursor, int limit);
        Task<AccountRequisites> GetByIdAsync(long id);
        
    }
}
