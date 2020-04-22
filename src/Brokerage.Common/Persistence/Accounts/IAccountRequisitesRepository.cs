using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public interface IAccountRequisitesRepository
    {
        Task<long> GetNextIdAsync();
        Task AddOrIgnoreAsync(AccountRequisites requisites);
        Task<AccountRequisites> GetByAccountAsync(long accountId);
        Task<IReadOnlyCollection<AccountRequisites>> GetAnyOfAsync(ISet<AccountRequisitesId> ids);
        Task<IReadOnlyCollection<AccountRequisites>> GetAllAsync(long? cursor, int limit);
        Task<AccountRequisites> GetAsync(long id);
        
    }
}
