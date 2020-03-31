using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public class BrokerAccountsBalancesRepository : IBrokerAccountsBalancesRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BrokerAccountsBalancesRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<BrokerAccountBalances> GetAsync(long brokerAccountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccountBalances
                .FirstAsync(x => x.BrokerAccountId == brokerAccountId);

            return MapToDomain(entity);
        }

        public async Task UpdateAsync(BrokerAccountBalances brokerAccountBalances)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(brokerAccountBalances);
            
            context.BrokerAccountBalances.Update(entity);
            
            await context.SaveChangesAsync();
        }

        public async Task<BrokerAccountBalances> AddOrGetAsync(BrokerAccountBalances brokerAccountBalances)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(brokerAccountBalances);

            context.BrokerAccountBalances.Add(newEntity);

            try
            {
                await context.SaveChangesAsync();

                return MapToDomain(newEntity);
            }
            catch (DbUpdateException e) //Check that request was already processed (by constraint)
                when (e.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == "23505")
            {
                var entity = await context
                    .BrokerAccountBalances
                    .FirstAsync(x => x.BrokerAccountId == brokerAccountBalances.BrokerAccountId);

                return MapToDomain(entity);
            }
        }

        private static BrokerAccountBalancesEntity MapToEntity(BrokerAccountBalances brokerAccountBalances)
        {
            var newEntity = new BrokerAccountBalancesEntity()
            {
                BrokerAccountId = brokerAccountBalances.BrokerAccountId,
                AssetId = brokerAccountBalances.AssetId,
                AvailableBalance = brokerAccountBalances.AvailableBalance,
                AvailableBalanceUpdateDateTime = brokerAccountBalances.AvailableBalanceUpdateDateTime,
                BrokerAccountBalancesId = brokerAccountBalances.Id,
                OwnedBalance = brokerAccountBalances.OwnedBalance,
                OwnedBalanceUpdateDateTime = brokerAccountBalances.OwnedBalanceUpdateDateTime,
                PendingBalance = brokerAccountBalances.PendingBalance,
                PendingBalanceUpdateDateTime = brokerAccountBalances.PendingBalanceUpdateDateTime,
                ReservedBalance = brokerAccountBalances.ReservedBalance,
                ReservedBalanceUpdateDateTime = brokerAccountBalances.ReservedBalanceUpdateDateTime,
                Version = brokerAccountBalances.Version
            };

            return newEntity;
        }

        private static BrokerAccountBalances MapToDomain(BrokerAccountBalancesEntity brokerAccountBalancesEntity)
        {
            var brokerAccountBalances = BrokerAccountBalances.Restore(
                brokerAccountBalancesEntity.BrokerAccountBalancesId,
                brokerAccountBalancesEntity.Version,
                brokerAccountBalancesEntity.BrokerAccountId,
                brokerAccountBalancesEntity.AssetId,
                brokerAccountBalancesEntity.OwnedBalance,
                brokerAccountBalancesEntity.AvailableBalance,
                brokerAccountBalancesEntity.PendingBalance,
                brokerAccountBalancesEntity.ReservedBalance,
                brokerAccountBalancesEntity.OwnedBalanceUpdateDateTime.UtcDateTime,
                brokerAccountBalancesEntity.AvailableBalanceUpdateDateTime.UtcDateTime,
                brokerAccountBalancesEntity.PendingBalanceUpdateDateTime.UtcDateTime,
                brokerAccountBalancesEntity.ReservedBalanceUpdateDateTime.UtcDateTime
                );

            return brokerAccountBalances;
        }
    }
}
