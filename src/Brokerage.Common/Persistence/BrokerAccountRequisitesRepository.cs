using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccountRequisites;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence
{
    public class BrokerAccountRequisitesRepository : IBrokerAccountRequisitesRepository
    {
        private readonly DbContextOptionsBuilder<BrokerageContext> _dbContextOptionsBuilder;

        public BrokerAccountRequisitesRepository(DbContextOptionsBuilder<BrokerageContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }


        public async Task<BrokerAccountRequisites> GetAsync(string brokerAccountRequisitesId)
        {
            long.TryParse(brokerAccountRequisitesId, out var id);
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccountsRequisites
                .FindAsync(id);

            return MapToDomain(entity);
        }

        public async Task<BrokerAccountRequisites> AddOrGetAsync(BrokerAccountRequisites brokerAccount)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

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
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);
            var entity = MapToEntity(brokerAccount);
            context
                .BrokerAccountsRequisites
                .Update(entity);
            await context.SaveChangesAsync();
        }

        private BrokerAccountRequisitesEntity MapToEntity(BrokerAccountRequisites brokerAccount)
        {
            return new BrokerAccountRequisitesEntity()
            {
                RequestId = brokerAccount.RequestId,
                BlockchainId = brokerAccount.BlockchainId,
                BrokerAccountId = brokerAccount.BrokerAccountId,
                Address = brokerAccount.Address,
                Id = brokerAccount.Id
            };
        }

        private BrokerAccountRequisites MapToDomain(BrokerAccountRequisitesEntity brokerAccountRequisitesEntity)
        {
            if (brokerAccountRequisitesEntity == null)
                return null;

            var brokerAccount = BrokerAccountRequisites.Restore(
                brokerAccountRequisitesEntity.RequestId,
                brokerAccountRequisitesEntity.Id,
                brokerAccountRequisitesEntity.BrokerAccountId,
                brokerAccountRequisitesEntity.BlockchainId,
                brokerAccountRequisitesEntity.Address
            );

            return brokerAccount;
        }
    }
}
