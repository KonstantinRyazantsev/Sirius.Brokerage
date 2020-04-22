using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.Accounts
{
    public class AccountRequisitesRepository : IAccountRequisitesRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public AccountRequisitesRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.AccountRequisites, nameof(AccountRequisites.Id));
        }

        public async Task AddOrIgnoreAsync(AccountRequisites requisites)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = MapToEntity(requisites);

            context.AccountRequisites.Add(entity);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        public async Task<AccountRequisites> GetByAccountAsync(long accountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var requisites = await context
                .AccountRequisites
                .FirstAsync(x => x.AccountId == accountId);

            return MapToDomain(requisites);
        }

        public async Task<IReadOnlyCollection<AccountRequisites>> GetAnyOfAsync(ISet<AccountRequisitesId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var idStrings = ids.Select(x => x.ToString());

            var query = context
                .AccountRequisites
                .Where(x => idStrings.Contains(x.NaturalId));

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<AccountRequisites>> GetAllAsync(long? cursor, int limit)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.AccountRequisites.AsQueryable();

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

        public async Task<AccountRequisites> GetAsync(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var requisites = await context
                .AccountRequisites
                .FirstAsync(x => x.Id == id);

            return MapToDomain(requisites);
        }

        private static AccountRequisitesEntity MapToEntity(AccountRequisites requisites)
        {
            return new AccountRequisitesEntity
            {
                Address = requisites.NaturalId.Address,
                Id = requisites.Id,
                NaturalId = requisites.NaturalId.ToString(),
                AccountId = requisites.AccountId,
                BrokerAccountId = requisites.BrokerAccountId,
                BlockchainId = requisites.NaturalId.BlockchainId,
                Tag = requisites.NaturalId.Tag,
                TagType = requisites.NaturalId.TagType,
                CreatedAt = requisites.CreatedAt
            };
        }

        private static AccountRequisites MapToDomain(AccountRequisitesEntity entity)
        {
            var brokerAccount = AccountRequisites.Restore(
                entity.Id,
                new AccountRequisitesId(entity.BlockchainId,
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
