﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence.Accounts;

namespace BrokerageTests.Repositories
{
    public class InMemoryAccountRequisitesRepository : IAccountRequisitesRepository
    {
        private long _idCounter = 0;
        private readonly List<AccountRequisites> _storage;
        public InMemoryAccountRequisitesRepository()
        {
            _storage = new List<AccountRequisites>(5);
        }

        public Task<IReadOnlyCollection<AccountRequisites>> GetByAccountAsync(long accountId,
            int limit,
            long? cursor,
            bool sortAsc)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<AccountRequisites>> GetByAddressesAsync(string blockchainId, IReadOnlyCollection<string> addresses)
        {
            return Task.FromResult<IReadOnlyCollection<AccountRequisites>>(
                _storage.Where(x => x.BlockchainId == blockchainId &&
                                    addresses.Contains(x.Address))
                .ToArray());
        }

        public Task<AccountRequisites> AddOrGetAsync(AccountRequisites requisites)
        {
            _idCounter++;
            _storage.Add(AccountRequisites.Restore(
                requisites.RequestId,
                _idCounter,
                requisites.AccountId,
                requisites.BrokerAccountId,
                requisites.BlockchainId,
                requisites.Address,
                requisites.Tag,
                requisites.TagType,
                requisites.CreationDateTime));

            return Task.FromResult(_storage.Last());
        }

        public Task UpdateAsync(AccountRequisites requisites)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<AccountRequisites>> GetAllAsync(long? cursor, int limit)
        {
            throw new System.NotImplementedException();
        }
    }
}