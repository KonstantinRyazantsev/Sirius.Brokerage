//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Brokerage.Common.Domain.BrokerAccounts;
//using Brokerage.Common.Persistence.BrokerAccount;

//namespace BrokerageTests.Repositories
//{
//    public class InMemoryBrokerAccountDetailsRepository : IBrokerAccountDetailsRepository
//    {
//        private long _idCounter = 0;
//        private readonly List<BrokerAccountDetails> _storage;

//        public InMemoryBrokerAccountDetailsRepository()
//        {
//            _storage = new List<BrokerAccountDetails>(5);
//        }

//        public Task<IReadOnlyCollection<BrokerAccountDetails>> GetAllAsync(long? brokerAccountId,
//            int limit,
//            long? cursor,
//            bool sortAsc,
//            string blockchainId,
//            string address)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<BrokerAccountDetails> AddOrGetAsync(BrokerAccountDetails brokerAccountDetails)
//        {
//            _idCounter++;
//            _storage.Add(BrokerAccountDetails.Restore(
//                brokerAccountDetails.RequestId,
//                _idCounter,
//                brokerAccountDetails.BrokerAccountId,
//                brokerAccountDetails.BlockchainId,
//                brokerAccountDetails.Address,
//                brokerAccountDetails.CreatedAt));

//            return Task.FromResult(_storage.Last());
//        }

//        public Task<IReadOnlyCollection<BrokerAccountDetails>> GetAnyOfAsync(string blockchainId, IReadOnlyCollection<string> addresses)
//        {
//            return Task.FromResult<IReadOnlyCollection<BrokerAccountDetails>>(
//                _storage.Where(x => x.BlockchainId == blockchainId &&
//                                    addresses.Contains(x.Address))
//                    .ToArray());
//        }

//        public Task AddOrIgnoreAsync(BrokerAccountDetails brokerAccount)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<BrokerAccountDetails> GetAsync(long brokerAccountDetailsId)
//        {
//            return Task.FromResult(_storage.First(x => x.Id == brokerAccountDetailsId));
//        }

//        public Task<BrokerAccountDetails> GetActiveAsync(long brokerAccountId, string blockchainId)
//        {
//            return Task.FromResult(_storage.OrderByDescending(x => x.Id)
//                .First(x => x.BrokerAccountId == brokerAccountId &&
//                            x.BlockchainId == blockchainId));
//        }
//    }
//}
