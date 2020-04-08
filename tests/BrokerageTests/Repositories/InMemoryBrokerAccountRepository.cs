using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.BrokerAccount;

namespace BrokerageTests.Repositories
{
    public class InMemoryBrokerAccountRepository : IBrokerAccountsRepository
    {
        private long _id = 0;
        private readonly List<BrokerAccount> _storage;

        public InMemoryBrokerAccountRepository()
        {
            _storage = new List<BrokerAccount>();
        }
        public Task<BrokerAccount> GetAsync(long brokerAccountId)
        {
            return Task.FromResult(_storage.First(x => x.BrokerAccountId == brokerAccountId));
        }

        public Task<BrokerAccount> AddOrGetAsync(BrokerAccount brokerAccount)
        {
            _id++;
            var account = BrokerAccount.Restore(_id,
                brokerAccount.Name,
                brokerAccount.TenantId,
                brokerAccount.CreationDateTime,
                brokerAccount.BlockingDateTime,
                brokerAccount.ActivationDateTime,
                brokerAccount.State,
                brokerAccount.RequestId);
            _storage.Add(account);

            return Task.FromResult(account);
        }

        public Task UpdateAsync(BrokerAccount brokerAccount)
        {
            throw new System.NotImplementedException();
        }
    }
}
