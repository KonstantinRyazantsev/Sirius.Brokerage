using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.BrokerAccount;

namespace BrokerageTests.Repositories
{
    public class InMemoryBrokerAccountRequisitesRepository : IBrokerAccountRequisitesRepository
    {
        private long _idCounter = 0;
        private readonly List<BrokerAccountRequisites> _storage;

        public InMemoryBrokerAccountRequisitesRepository()
        {
            _storage = new List<BrokerAccountRequisites>(5);
        }

        public Task<IReadOnlyCollection<BrokerAccountRequisites>> GetAllAsync(long? brokerAccountId,
            int limit,
            long? cursor,
            bool sortAsc,
            string blockchainId,
            string address)
        {
            throw new System.NotImplementedException();
        }

        public Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccountRequisites)
        {
            _idCounter++;
            _storage.Add(BrokerAccountRequisites.Restore(
                brokerAccountRequisites.RequestId,
                _idCounter,
                brokerAccountRequisites.BrokerAccountId,
                brokerAccountRequisites.BlockchainId,
                brokerAccountRequisites.Address,
                brokerAccountRequisites.CreatedAt));

            return Task.FromResult(_storage.Last());
        }

        public Task<IReadOnlyCollection<BrokerAccountRequisites>> GetAnyOfAsync(string blockchainId, IReadOnlyCollection<string> addresses)
        {
            return Task.FromResult<IReadOnlyCollection<BrokerAccountRequisites>>(
                _storage.Where(x => x.BlockchainId == blockchainId &&
                                    addresses.Contains(x.Address))
                    .ToArray());
        }

        public Task UpdateAsync(BrokerAccountRequisites brokerAccount)
        {
            throw new System.NotImplementedException();
        }

        public Task<BrokerAccountRequisites> GetByIdAsync(long brokerAccountRequisitesId)
        {
            return Task.FromResult(_storage.First(x => x.Id == brokerAccountRequisitesId));
        }

        public Task<BrokerAccountRequisites> GetActualByBrokerAccountIdAndBlockchainAsync(long brokerAccountId, string blockchainId)
        {
            return Task.FromResult(_storage.OrderByDescending(x => x.Id)
                .First(x => x.BrokerAccountId == brokerAccountId &&
                            x.BlockchainId == blockchainId));
        }
    }
}
