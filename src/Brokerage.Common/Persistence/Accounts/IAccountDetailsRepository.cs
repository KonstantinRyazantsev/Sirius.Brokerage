using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public interface IAccountDetailsRepository
    {
        Task<long> GetNextIdAsync();
        Task AddOrIgnoreAsync(AccountDetails details);
        Task<AccountDetails> GetByAccountAsync(long accountId);
        Task<long> GetCountByAccountIdAsync(long accountId);
        Task<IReadOnlyCollection<AccountDetails>> GetAnyOfAsync(ISet<AccountDetailsId> ids);
        Task<IReadOnlyCollection<AccountDetails>> GetAllAsync(long? cursor, int limit);
        Task<AccountDetails> GetAsync(long id);
        
    }
}
