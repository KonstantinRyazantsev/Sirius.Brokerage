using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public interface IAccountsRepository
    {
        Task<Account> Get(long accountId);
        Task<Account> GetOrDefault(long accountId);
        Task Add(Account account);
        Task Update(Account account);
    }
}
