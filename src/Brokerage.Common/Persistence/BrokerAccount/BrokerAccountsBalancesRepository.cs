using System.Collections.Generic;
using System.Linq;
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
        
        public async Task<BrokerAccountBalances> GetOrDefaultAsync(BrokerAccountBalancesId id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccountBalances
                .FirstOrDefaultAsync(x => x.NaturalId == id.ToString());

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task SaveAsync(string updatePrefix, IReadOnlyCollection<BrokerAccountBalances> balances)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            await using var transaction = context.Database.BeginTransaction();

            var entities = balances
                .Select(MapToEntity);

            foreach (var balance in balances)
            {
                context.BrokerAccountBalancesUpdate.Add(new BrokerAccountBalancesUpdateEntity
                {
                    UpdateId = $"{updatePrefix}-{balance.NaturalId}"
                });    
            }

            foreach (var entity in entities)
            {
                if (entity.Version == default)
                {
                    context.BrokerAccountBalances.Add(entity);
                }
                else
                {
                    context.BrokerAccountBalances.Update(entity);
                }
            }

            try
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        public async Task<IReadOnlyCollection<BrokerAccountBalances>> GetAnyOfAsync(ISet<BrokerAccountBalancesId> ids)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var idStrings = ids.Select(x => x.ToString()).ToArray();

            var query = context
                .BrokerAccountBalances
                .Where(x => idStrings.Contains(x.NaturalId));
            
            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.BrokerAccountBalances,  nameof(BrokerAccountBalancesEntity.Id));
        }

        private static BrokerAccountBalancesEntity MapToEntity(BrokerAccountBalances brokerAccountBalances)
        {
            var newEntity = new BrokerAccountBalancesEntity()
            {
                BrokerAccountId = brokerAccountBalances.NaturalId.BrokerAccountId,
                AssetId = brokerAccountBalances.NaturalId.AssetId,
                AvailableBalance = brokerAccountBalances.AvailableBalance,
                AvailableBalanceUpdatedAt = brokerAccountBalances.AvailableBalanceUpdatedAt,
                Id = brokerAccountBalances.Id,
                NaturalId = brokerAccountBalances.NaturalId.ToString(),
                OwnedBalance = brokerAccountBalances.OwnedBalance,
                OwnedBalanceUpdatedAt = brokerAccountBalances.OwnedBalanceUpdatedAt,
                PendingBalance = brokerAccountBalances.PendingBalance,
                PendingBalanceUpdatedAt = brokerAccountBalances.PendingBalanceUpdatedAt,
                ReservedBalance = brokerAccountBalances.ReservedBalance,
                ReservedBalanceUpdateDatedAt = brokerAccountBalances.ReservedBalanceUpdatedAt,
                Version = brokerAccountBalances.Version,
                Sequence = brokerAccountBalances.Sequence
            };

            return newEntity;
        }

        private static BrokerAccountBalances MapToDomain(BrokerAccountBalancesEntity brokerAccountBalancesEntity)
        {
            var brokerAccountBalances = BrokerAccountBalances.Restore(
                brokerAccountBalancesEntity.Id,
                brokerAccountBalancesEntity.Sequence,
                brokerAccountBalancesEntity.Version,
                new BrokerAccountBalancesId(brokerAccountBalancesEntity.BrokerAccountId, brokerAccountBalancesEntity.AssetId),
                brokerAccountBalancesEntity.OwnedBalance,
                brokerAccountBalancesEntity.AvailableBalance,
                brokerAccountBalancesEntity.PendingBalance,
                brokerAccountBalancesEntity.ReservedBalance,
                brokerAccountBalancesEntity.OwnedBalanceUpdatedAt.UtcDateTime,
                brokerAccountBalancesEntity.AvailableBalanceUpdatedAt.UtcDateTime,
                brokerAccountBalancesEntity.PendingBalanceUpdatedAt.UtcDateTime,
                brokerAccountBalancesEntity.ReservedBalanceUpdateDatedAt.UtcDateTime);

            return brokerAccountBalances;
        }
    }
}
