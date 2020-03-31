using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
                .FirstAsync(x => x.BrokerAccountId == brokerAccountId);

            return MapToDomain(entity);
        }

        public async Task UpdateAsync(Domain.BrokerAccounts.BrokerAccount brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(brokerAccount);
            
            context.BrokerAccounts.Update(entity);
            
            await context.SaveChangesAsync();
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
                CreationDateTime = brokerAccount.CreationDateTime,
                TenantId = brokerAccount.TenantId,
                RequestId = brokerAccount.RequestId,
                ActivationDateTime = brokerAccount.ActivationDateTime,
                BrokerAccountId = brokerAccount.BrokerAccountId,
                BlockingDateTime = brokerAccount.BlockingDateTime
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
                brokerAccountEntity.BrokerAccountId,
                brokerAccountEntity.Name,
                brokerAccountEntity.TenantId,
                brokerAccountEntity.CreationDateTime.UtcDateTime,
                brokerAccountEntity.BlockingDateTime?.UtcDateTime,
                brokerAccountEntity.ActivationDateTime?.UtcDateTime,
                state,
                brokerAccountEntity.RequestId
            );

            return brokerAccount;
        }
    }
}
