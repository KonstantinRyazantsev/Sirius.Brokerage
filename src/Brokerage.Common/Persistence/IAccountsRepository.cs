using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence
{
    public interface IAccountsRepository
    {
        Task<Account> GetAsync(long accountId);

        Task<Account> AddOrGetAsync(Account account);

        Task UpdateAsync(Account account);
    }
}
