using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public class BrokerAccountDetailsRepository : IBrokerAccountDetailsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BrokerAccountDetailsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }
        
        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.BrokerAccountDetails, nameof(BrokerAccountDetails.Id));
        }

        public async Task<IReadOnlyCollection<BrokerAccountDetails>> GetByBrokerAccountAsync(long brokerAccountId,
            int limit,
            long? cursor)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context
                .BrokerAccountsDetails
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

        public async Task<IReadOnlyCollection<BrokerAccountDetails>> GetAnyOfAsync(ISet<BrokerAccountDetailsId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var idStrings = ids.Select(x => x.ToString()).ToArray();

            var query = context
                .BrokerAccountsDetails
                .Where(x => idStrings.Contains(x.NaturalId));
            
            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyDictionary<ActiveBrokerAccountDetailsId, BrokerAccountDetails>> GetActiveAsync(
            ISet<ActiveBrokerAccountDetailsId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entities = new List<BrokerAccountDetailsEntity>();

            foreach (var id in ids)
            {
                var entity = await context
                    .BrokerAccountsDetails
                    .Where(x => x.ActiveId == id.ToString())
                    .OrderByDescending(x => x.Id)
                    .FirstAsync();

                entities.Add(entity);
            }

            return entities
                .Select(MapToDomain)
                .ToDictionary(
                    x => new ActiveBrokerAccountDetailsId(x.NaturalId.BlockchainId, x.BrokerAccountId), 
                    x => x);
        }

        public async Task<BrokerAccountDetails> GetActiveAsync(ActiveBrokerAccountDetailsId id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var result = await context
                .BrokerAccountsDetails
                .Where(x => x.ActiveId == id.ToString())
                .OrderByDescending(x => x.Id)
                .FirstAsync();

            return MapToDomain(result);
        }

        public async Task AddOrIgnoreAsync(BrokerAccountDetails brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(brokerAccount);

            try
            {
                await context.BrokerAccountsDetails.AddAsync(entity);

                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        public async Task<BrokerAccountDetails> GetAsync(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var details = await context
                .BrokerAccountsDetails
                .FirstAsync(x => x.Id == id);

            return MapToDomain(details);
        }

        private static BrokerAccountDetailsEntity MapToEntity(BrokerAccountDetails details)
        {
            return new BrokerAccountDetailsEntity
            {
                BlockchainId = details.NaturalId.BlockchainId,
                TenantId = details.TenantId,
                BrokerAccountId = details.BrokerAccountId,
                Address = details.NaturalId.Address,
                Id = details.Id,
                NaturalId = details.NaturalId.ToString(),
                ActiveId = new ActiveBrokerAccountDetailsId(details.NaturalId.BlockchainId, details.BrokerAccountId).ToString(),
                CreatedAt = details.CreatedAt
            };
        }

        private static BrokerAccountDetails MapToDomain(BrokerAccountDetailsEntity entity)
        {
            if (entity == null)
                return null;

            var brokerAccount = BrokerAccountDetails.Restore(
                entity.Id,
                new BrokerAccountDetailsId(entity.BlockchainId, entity.Address),
                entity.TenantId,
                entity.BrokerAccountId,
                entity.CreatedAt.UtcDateTime
            );

            return brokerAccount;
        }
    }
}
