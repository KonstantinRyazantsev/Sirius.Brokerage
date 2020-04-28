//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Brokerage.Common.Domain.Accounts;
//using Brokerage.Common.Persistence.Accounts;

//namespace BrokerageTests.Repositories
//{
//    public class InMemoryAccountDetailsRepository : IAccountDetailsRepository
//    {
//        private long _idCounter = 0;
//        private readonly List<AccountDetails> _storage;
//        public InMemoryAccountDetailsRepository()
//        {
//            _storage = new List<AccountDetails>(5);
//        }

//        public Task<IReadOnlyCollection<AccountDetails>> GetByAccountAsync(long accountId,
//            int limit,
//            long? cursor,
//            bool sortAsc)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<IReadOnlyCollection<AccountDetails>> GetAnyOfAsync(string blockchainId, IReadOnlyCollection<AccountDetailsId> ids)
//        {
//            return Task.FromResult<IReadOnlyCollection<AccountDetails>>(
//                _storage.Where(x => x.BlockchainId == blockchainId &&
//                                    addresses.Contains(x.Address))
//                .ToArray());
//        }

//        public Task<AccountDetails> AddOrGetAsync(AccountDetails requisites)
//        {
//            _idCounter++;
//            _storage.Add(AccountDetails.Restore(
//                requisites.RequestId,
//                _idCounter,
//                requisites.AccountId,
//                requisites.BrokerAccountId,
//                requisites.BlockchainId,
//                requisites.Address,
//                requisites.Tag,
//                requisites.TagType,
//                requisites.CreatedAt));

//            return Task.FromResult(_storage.Last());
//        }

//        public Task UpdateAsync(AccountDetails requisites)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<IReadOnlyCollection<AccountDetails>> GetAllAsync(long? cursor, int limit)
//        {
//            throw new System.NotImplementedException();
//        }

//        public Task<AccountDetails> GetAsync(long id)
//        {
//            return Task.FromResult(_storage.First(x => x.Id == id));
//        }
//    }
//}
