using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public interface IAccountDetailsRepository
    {
        Task<IReadOnlyCollection<AccountDetails>> GetAnyOf(ISet<AccountDetailsId> ids);
        Task<IReadOnlyCollection<AccountDetails>> GetAll(long? cursor, int limit);
        Task<AccountDetails> Get(long id);
        Task Add(AccountDetails details);
        Task<long> GetCountByAccountId(long accountId);
    }
}
