using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.Accounts
{
    public class AccountDetailsRepository : IAccountDetailsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public AccountDetailsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.AccountDetails, nameof(AccountDetails.Id));
        }

        public async Task AddOrIgnoreAsync(AccountDetails details)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = MapToEntity(details);

            context.AccountDetails.Add(entity);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        public async Task<AccountDetails> GetByAccountAsync(long accountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var requisites = await context
                .AccountDetails
                .FirstAsync(x => x.AccountId == accountId);

            return MapToDomain(requisites);
        }

        public async Task<IReadOnlyCollection<AccountDetails>> GetAnyOfAsync(ISet<AccountDetailsId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var idStrings = ids.Select(x => x.ToString());

            var query = context
                .AccountDetails
                .Where(x => idStrings.Contains(x.NaturalId));

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<AccountDetails>> GetAllAsync(long? cursor, int limit)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.AccountDetails.AsQueryable();

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

        public async Task<AccountDetails> GetAsync(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var requisites = await context
                .AccountDetails
                .FirstAsync(x => x.Id == id);

            return MapToDomain(requisites);
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
    }
}
