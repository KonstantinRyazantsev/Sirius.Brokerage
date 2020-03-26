using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence
{
    public interface IAccountRepository
    {
        Task<Account> GetOrDefaultAsync(long accountId);

        Task<Account> AddOrGetAsync(Account account);

        Task UpdateAsync(Account account);
    }
}
