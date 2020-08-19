using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Microsoft.EntityFrameworkCore;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    public class BrokerAccountsRepository : IBrokerAccountsRepository
    {
        private readonly DatabaseContext _dbContext;

        public BrokerAccountsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BrokerAccount> Get(long brokerAccountId)
        {
            var entity = await _dbContext
                .BrokerAccounts
                .FirstAsync(x => x.Id == brokerAccountId);

            return MapToDomain(entity);
        }

        public async Task<BrokerAccount> GetOrDefault(long brokerAccountId)
        {
            var entity = await _dbContext
                .BrokerAccounts
                .FirstOrDefaultAsync(x => x.Id == brokerAccountId);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task Add(BrokerAccount brokerAccount)
        {
            var newEntity = MapToEntity(brokerAccount);

            _dbContext.BrokerAccounts.Add(newEntity);

            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(BrokerAccount brokerAccount)
        {
            var entity = MapToEntity(brokerAccount);
            
            _dbContext.BrokerAccounts.Update(entity);
            
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<BrokerAccount>> GetAllOf(IReadOnlyCollection<long> brokerAccountIds)
        {
            var entities = await _dbContext.BrokerAccounts
                .Where(x => brokerAccountIds.Contains(x.Id))
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToArray();
        }

        private static BrokerAccountEntity MapToEntity(BrokerAccount brokerAccount)
        {
            var state = brokerAccount.State switch
            {
                BrokerAccountState.Active => BrokerAccountStateEnum.Active,
                BrokerAccountState.Blocked => BrokerAccountStateEnum.Blocked,
                BrokerAccountState.Creating => BrokerAccountStateEnum.Creating,

                _ => throw new ArgumentOutOfRangeException()
            };

            var newEntity = new BrokerAccountEntity
            {
                Name = brokerAccount.Name,
                State = state,
                CreatedAt = brokerAccount.CreatedAt,
                TenantId = brokerAccount.TenantId,
                UpdatedAt = brokerAccount.UpdatedAt,
                VaultId = brokerAccount.VaultId,
                Id = brokerAccount.Id,
                Sequence = brokerAccount.Sequence
            };
            return newEntity;
        }

        private static BrokerAccount MapToDomain(BrokerAccountEntity brokerAccountEntity)
        {
            var state = brokerAccountEntity.State switch
            {
                BrokerAccountStateEnum.Active => BrokerAccountState.Active,
                BrokerAccountStateEnum.Blocked => BrokerAccountState.Blocked,
                BrokerAccountStateEnum.Creating => BrokerAccountState.Creating,
                _ => throw new ArgumentOutOfRangeException($"{brokerAccountEntity.State} is not processed")
            };

            var brokerAccount = BrokerAccount.Restore(
                brokerAccountEntity.Id,
                brokerAccountEntity.Name,
                brokerAccountEntity.TenantId,
                brokerAccountEntity.CreatedAt.UtcDateTime,
                brokerAccountEntity.UpdatedAt.UtcDateTime,
                state,
                brokerAccountEntity.VaultId,
                brokerAccountEntity.Sequence
            );

            return brokerAccount;
        }
    }
}
