using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence
{
    public class BrokerAccountRequisitesRepository : IBrokerAccountRequisitesRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BrokerAccountRequisitesRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }


        public async Task<IReadOnlyCollection<BrokerAccountRequisites>> SearchAsync(
            long brokerAccountId,
            int limit,
            long? cursor,
            bool sortAsc)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context
                .BrokerAccountsRequisites
                .Where(x => x.BrokerAccountId == brokerAccountId);

            if (sortAsc)
            {
                if (cursor != null)
                {
                    // ReSharper disable once StringCompareToIsCultureSpecific
                    query = query.Where(x => cursor < 0);
                }

                query = query.OrderBy(x => x.Id);
            }
            else
            {
                if (cursor != null)
                {
                    // ReSharper disable once StringCompareToIsCultureSpecific
                    query = query.Where(x => cursor > 0);
                }

                query = query.OrderByDescending(x => x.Id);
            }

            query = query.Take(limit);

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(brokerAccount);

            context.BrokerAccountsRequisites.Add(newEntity);

            try
            {
                await context.SaveChangesAsync();

                return MapToDomain(newEntity);
            }
            catch (DbUpdateException e) //Check that request was already processed (by constraint)
                when (e.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == "23505" &&
                      pgEx.ConstraintName == "IX_BrokerAccountRequisites_RequestId")
            {
                var entity = await context
                    .BrokerAccountsRequisites
                    .FirstOrDefaultAsync(x => x.RequestId == brokerAccount.RequestId);

                return MapToDomain(entity);
            }
        }

        public async Task UpdateAsync(BrokerAccountRequisites brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var entity = MapToEntity(brokerAccount);
            context
                .BrokerAccountsRequisites
                .Update(entity);
            await context.SaveChangesAsync();
        }

        private static BrokerAccountRequisitesEntity MapToEntity(BrokerAccountRequisites requisites)
        {
            return new BrokerAccountRequisitesEntity
            {
                RequestId = requisites.RequestId,
                BlockchainId = requisites.BlockchainId,
                BrokerAccountId = requisites.BrokerAccountId,
                Address = requisites.Address,
                Id = requisites.Id,
                CreationDateTime = requisites.CreationDateTime
            };
        }

        private BrokerAccountRequisites MapToDomain(BrokerAccountRequisitesEntity entity)
        {
            if (entity == null)
                return null;

            var brokerAccount = BrokerAccountRequisites.Restore(
                entity.RequestId,
                entity.Id,
                entity.BrokerAccountId,
                entity.BlockchainId,
                entity.Address,
                entity.CreationDateTime.UtcDateTime
            );

            return brokerAccount;
        }
    }
}
