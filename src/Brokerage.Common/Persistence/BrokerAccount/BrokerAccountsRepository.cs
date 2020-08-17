using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    public class BrokerAccountsRepository : IBrokerAccountsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BrokerAccountsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Domain.BrokerAccounts.BrokerAccount> GetAsync(long brokerAccountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccounts
                .FirstAsync(x => x.Id == brokerAccountId);

            return MapToDomain(entity);
        }

        public async Task<Domain.BrokerAccounts.BrokerAccount> GetOrDefaultAsync(long brokerAccountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccounts
                .FirstOrDefaultAsync(x => x.Id == brokerAccountId);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task UpdateAsync(Domain.BrokerAccounts.BrokerAccount brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(brokerAccount);
            
            context.BrokerAccounts.Update(entity);
            
            await context.SaveChangesAsync();
        }

        public async Task<long> GetCountByBrokerAccountIdAsync(long brokerAccountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var count = await context.BrokerAccountsDetails.Where(x => x.BrokerAccountId == brokerAccountId).CountAsync();

            return count;
        }

        public async Task<Domain.BrokerAccounts.BrokerAccount> AddOrGetAsync(Domain.BrokerAccounts.BrokerAccount brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(brokerAccount);

            context.BrokerAccounts.Add(newEntity);

            try
            {
                await context.SaveChangesAsync();

                return MapToDomain(newEntity);
            }
            catch (DbUpdateException e) //Check that request was already processed (by constraint)
                when (e.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == "23505" &&
                      pgEx.ConstraintName == "IX_BrokerAccount_RequestId")
            {
                var entity = await context
                    .BrokerAccounts
                    .FirstAsync(x => x.RequestId == brokerAccount.RequestId);

                return MapToDomain(entity);
            }
        }

        private static BrokerAccountEntity MapToEntity(Domain.BrokerAccounts.BrokerAccount brokerAccount)
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
                RequestId = brokerAccount.RequestId,
                UpdatedAt = brokerAccount.UpdatedAt,
                VaultId = brokerAccount.VaultId,
                Id = brokerAccount.Id
            };
            return newEntity;
        }

        private static Domain.BrokerAccounts.BrokerAccount MapToDomain(BrokerAccountEntity brokerAccountEntity)
        {
            var state = brokerAccountEntity.State switch
            {
                BrokerAccountStateEnum.Active => BrokerAccountState.Active,
                BrokerAccountStateEnum.Blocked => BrokerAccountState.Blocked,
                BrokerAccountStateEnum.Creating => BrokerAccountState.Creating,
                _ => throw new ArgumentOutOfRangeException($"{brokerAccountEntity.State} is not processed")
            };

            var brokerAccount = Domain.BrokerAccounts.BrokerAccount.Restore(
                brokerAccountEntity.Id,
                brokerAccountEntity.Name,
                brokerAccountEntity.TenantId,
                brokerAccountEntity.CreatedAt.UtcDateTime,
                brokerAccountEntity.UpdatedAt.UtcDateTime,
                state,
                brokerAccountEntity.RequestId,
                brokerAccountEntity.VaultId,
                brokerAccountEntity.Sequence
            );

            return brokerAccount;
        }
    }
}
