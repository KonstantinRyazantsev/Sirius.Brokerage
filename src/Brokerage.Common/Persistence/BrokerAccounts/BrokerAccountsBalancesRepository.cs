using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    public class BrokerAccountsBalancesRepository : IBrokerAccountsBalancesRepository
    {
        private readonly DatabaseContext _dbContext;

        public BrokerAccountsBalancesRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BrokerAccountBalances> Get(BrokerAccountBalancesId id)
        {
            var brokerAccountBalances = await GetOrDefault(id);

            if (brokerAccountBalances == null)
            {
                throw new InvalidOperationException("Broker account balances entity is not found");
            }

            return brokerAccountBalances;
        }

        public async Task<BrokerAccountBalances> GetOrDefault(BrokerAccountBalancesId id)
        {
            var entity = await _dbContext
                .BrokerAccountBalances
                .FirstOrDefaultAsync(x => x.NaturalId == id.ToString());

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task Save(IReadOnlyCollection<BrokerAccountBalances> balances)
        {
            if (!balances.Any())
            {
                return;
            }

            var entities = balances.Select(MapToEntity);

            foreach (var entity in entities)
            {
                if (entity.Version == default)
                {
                    _dbContext.BrokerAccountBalances.Add(entity);
                }
                else
                {
                    _dbContext.BrokerAccountBalances.Update(entity);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<BrokerAccountBalances>> GetAnyOf(ISet<BrokerAccountBalancesId> ids)
        {
            if (ids.Count == 0)
            {
                return Array.Empty<BrokerAccountBalances>();
            }

            var idStrings = ids.Select(x => x.ToString()).ToArray();

            var query = _dbContext
                .BrokerAccountBalances
                .Where(x => idStrings.Contains(x.NaturalId));
            
            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        private static BrokerAccountBalancesEntity MapToEntity(BrokerAccountBalances brokerAccountBalances)
        {
            var newEntity = new BrokerAccountBalancesEntity()
            {
                BrokerAccountId = brokerAccountBalances.NaturalId.BrokerAccountId,
                AssetId = brokerAccountBalances.NaturalId.AssetId,
                AvailableBalance = brokerAccountBalances.AvailableBalance,
                UpdatedAt = brokerAccountBalances.UpdatedAt,
                Id = brokerAccountBalances.Id,
                NaturalId = brokerAccountBalances.NaturalId.ToString(),
                OwnedBalance = brokerAccountBalances.OwnedBalance,
                CreatedAt = brokerAccountBalances.CreatedAt,
                PendingBalance = brokerAccountBalances.PendingBalance,
                ReservedBalance = brokerAccountBalances.ReservedBalance,
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
                brokerAccountBalancesEntity.CreatedAt.UtcDateTime,
                brokerAccountBalancesEntity.UpdatedAt.UtcDateTime);

            return brokerAccountBalances;
        }
    }
}
