using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence
{
    public class BrokerAccountRepository : IBrokerAccountRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BrokerAccountRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }


        public async Task<BrokerAccount> GetAsync(long brokerAccountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccounts
                .FirstOrDefaultAsync(x => x.BrokerAccountId == brokerAccountId);

            return MapToDomain(entity);
        }

        public async Task UpdateAsync(BrokerAccount brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var entity = MapToEntity(brokerAccount);
            context.BrokerAccounts.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task<BrokerAccount> AddOrGetAsync(BrokerAccount brokerAccount)
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
                    .FirstOrDefaultAsync(x => x.RequestId == brokerAccount.RequestId);

                return MapToDomain(entity);
            }
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

            var newEntity = new BrokerAccountEntity()
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

        private BrokerAccount MapToDomain(BrokerAccountEntity brokerAccountEntity)
        {
            if (brokerAccountEntity == null)
                return null;

            var state = brokerAccountEntity.State switch
            {
                BrokerAccountStateEnum.Active => BrokerAccountState.Active,
                BrokerAccountStateEnum.Blocked => BrokerAccountState.Blocked,
                BrokerAccountStateEnum.Creating => BrokerAccountState.Creating,
                _ => throw new ArgumentOutOfRangeException($"{brokerAccountEntity.State} is not processed")
            };

            var brokerAccount = BrokerAccount.Restore(
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
