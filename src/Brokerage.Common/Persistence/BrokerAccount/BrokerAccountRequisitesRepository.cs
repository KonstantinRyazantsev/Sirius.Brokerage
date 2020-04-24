using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public class BrokerAccountRequisitesRepository : IBrokerAccountRequisitesRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BrokerAccountRequisitesRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }
        
        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.BrokerAccountRequisites, nameof(BrokerAccountRequisites.Id));
        }

        public async Task<IReadOnlyCollection<BrokerAccountRequisites>> GetByBrokerAccountAsync(long brokerAccountId,
            int limit,
            long? cursor)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context
                .BrokerAccountsRequisites
                .Where(x => x.BrokerAccountId == brokerAccountId);
        
            if (cursor != null)
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                query = query.Where(x => cursor < 0);
            }

            query = query
                .OrderBy(x => x.Id)
                .Take(limit);

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<BrokerAccountRequisites>> GetAnyOfAsync(ISet<BrokerAccountRequisitesId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var idStrings = ids.Select(x => x.ToString()).ToArray();

            var query = context
                .BrokerAccountsRequisites
                .Where(x => idStrings.Contains(x.NaturalId));
            
            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyDictionary<ActiveBrokerAccountRequisitesId, BrokerAccountRequisites>> GetActiveAsync(
            ISet<ActiveBrokerAccountRequisitesId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entities = new List<BrokerAccountRequisitesEntity>();

            foreach (var id in ids)
            {
                var entity = await context
                    .BrokerAccountsRequisites
                    .Where(x => x.ActiveId == id.ToString())
                    .OrderByDescending(x => x.Id)
                    .FirstAsync();

                entities.Add(entity);
            }

            return entities
                .Select(MapToDomain)
                .ToDictionary(
                    x => new ActiveBrokerAccountRequisitesId(x.NaturalId.BlockchainId, x.BrokerAccountId), 
                    x => x);
        }

        public async Task<BrokerAccountRequisites> GetActiveAsync(ActiveBrokerAccountRequisitesId id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var result = await context
                .BrokerAccountsRequisites
                .Where(x => x.ActiveId == id.ToString())
                .OrderByDescending(x => x.Id)
                .FirstAsync();

            return MapToDomain(result);
        }

        public async Task AddOrIgnoreAsync(BrokerAccountRequisites brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(brokerAccount);

            try
            {
                await context.BrokerAccountsRequisites.AddAsync(entity);

                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        public async Task<BrokerAccountRequisites> GetAsync(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var requisites = await context
                .BrokerAccountsRequisites
                .FirstAsync(x => x.Id == id);

            return MapToDomain(requisites);
        }

        private static BrokerAccountRequisitesEntity MapToEntity(BrokerAccountRequisites requisites)
        {
            return new BrokerAccountRequisitesEntity
            {
                BlockchainId = requisites.NaturalId.BlockchainId,
                TenantId = requisites.TenantId,
                BrokerAccountId = requisites.BrokerAccountId,
                Address = requisites.NaturalId.Address,
                Id = requisites.Id,
                NaturalId = requisites.NaturalId.ToString(),
                ActiveId = new ActiveBrokerAccountRequisitesId(requisites.NaturalId.BlockchainId, requisites.BrokerAccountId).ToString(),
                CreatedAt = requisites.CreatedAt
            };
        }

        private static BrokerAccountRequisites MapToDomain(BrokerAccountRequisitesEntity entity)
        {
            if (entity == null)
                return null;

            var brokerAccount = BrokerAccountRequisites.Restore(
                entity.Id,
                new BrokerAccountRequisitesId(entity.BlockchainId, entity.Address),
                entity.TenantId,
                entity.BrokerAccountId,
                entity.CreatedAt.UtcDateTime
            );

            return brokerAccount;
        }
    }
}
