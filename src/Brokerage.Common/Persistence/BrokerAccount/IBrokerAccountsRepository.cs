using System.Threading.Tasks;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public interface IBrokerAccountsRepository
    {
        Task<Domain.BrokerAccounts.BrokerAccount> GetAsync(long brokerAccountId);

        Task<Domain.BrokerAccounts.BrokerAccount> GetOrDefaultAsync(long brokerAccountId);

        Task<Domain.BrokerAccounts.BrokerAccount> AddOrGetAsync(Domain.BrokerAccounts.BrokerAccount brokerAccount);

        Task UpdateAsync(Domain.BrokerAccounts.BrokerAccount brokerAccount);
        Task<long> GetCountByBrokerAccountIdAsync(long brokerAccountId);
    }
}
