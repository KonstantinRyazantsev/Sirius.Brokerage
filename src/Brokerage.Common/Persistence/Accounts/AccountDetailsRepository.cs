using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.Accounts
{
    public class AccountDetailsRepository : IAccountDetailsRepository
    {
        private readonly DatabaseContext _dbContext;

        public AccountDetailsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<AccountDetails>> GetAnyOf(ISet<AccountDetailsId> ids)
        {
            var idStrings = ids.Select(x => x.ToString());

            var query = _dbContext
                .AccountDetails
                .Where(x => idStrings.Contains(x.NaturalId));

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<AccountDetails>> GetAll(long? cursor, int limit)
        {
            var query = _dbContext.AccountDetails.AsQueryable();

            if (cursor != null)
            {
                query = query.Where(x => x.Id > cursor);
            }

            query = query.OrderBy(x => x.Id);

            query = query.Take(limit);

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<AccountDetails> Get(long id)
        {
            var requisites = await _dbContext
                .AccountDetails
                .FirstAsync(x => x.Id == id);

            return MapToDomain(requisites);
        }

        public async Task Add(AccountDetails details)
        {
            var entity = MapToEntity(details);

            _dbContext.AccountDetails.Add(entity);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<long> GetCountByAccountId(long accountId)
        {
            var count = await _dbContext.AccountDetails.Where(x => x.AccountId == accountId).CountAsync();

            return count;
        }

        private static AccountDetails MapToDomain(AccountDetailsEntity entity)
        {
            var brokerAccount = AccountDetails.Restore(
                entity.Id,
                new AccountDetailsId(entity.BlockchainId,
                    entity.Address,
                    entity.Tag,
                    entity.TagType),
                entity.AccountId,
                entity.BrokerAccountId,
                entity.CreatedAt.UtcDateTime);

            return brokerAccount;
        }

        private static AccountDetailsEntity MapToEntity(AccountDetails details)
        {
            return new AccountDetailsEntity
            {
                Address = details.NaturalId.Address,
                Id = details.Id,
                NaturalId = details.NaturalId.ToString(),
                AccountId = details.AccountId,
                BrokerAccountId = details.BrokerAccountId,
                BlockchainId = details.NaturalId.BlockchainId,
                Tag = details.NaturalId.Tag,
                TagType = details.NaturalId.TagType,
                CreatedAt = details.CreatedAt
            };
        }
    }
}
