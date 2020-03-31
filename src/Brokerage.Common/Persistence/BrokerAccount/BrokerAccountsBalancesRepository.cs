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
        
        public async Task<BrokerAccountBalances> GetOrDefaultAsync(long brokerAccountId, long assetId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccountBalances
                .FirstOrDefaultAsync(x => x.BrokerAccountId == brokerAccountId && x.AssetId == assetId);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task SaveAsync(BrokerAccountBalances brokerAccountBalances)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(brokerAccountBalances);

            if (brokerAccountBalances.Id == default)
            {
                context.BrokerAccountBalances.Add(entity);
            }
            else
            {
                context.BrokerAccountBalances.Update(entity);
            }
            
            await context.SaveChangesAsync();
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
