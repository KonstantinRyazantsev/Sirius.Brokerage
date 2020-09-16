using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public interface IAccountsRepository
    {
        Task<Account> Get(long accountId);
        Task<Account> GetOrDefault(long accountId);
        Task Add(Account account);
        Task Update(Account account);
        Task<IReadOnlyCollection<Account>> GetForBrokerAccount(long brokerAccountId, long cursor, int limit);
        Task<int> GetCountForBrokerAccountId(long brokerAccountId, AccountState? accountState = null);
    }
}
