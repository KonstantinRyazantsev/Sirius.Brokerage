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
        private readonly DbContextOptionsBuilder<BrokerageContext> _dbContextOptionsBuilder;

        public BrokerAccountRepository(DbContextOptionsBuilder<BrokerageContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }


        public async Task<BrokerAccount> GetAsync(long brokerAccountId)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .BrokerAccounts
                .FirstOrDefaultAsync(x => x.BrokerAccountId == brokerAccountId);

            return MapToDomain(entity);
        }

        public async Task<BrokerAccount> AddOrGetAsync(
            string requestId,
            BrokerAccount brokerAccount)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var newEntity = new BrokerAccountEntity()
            {
                Name = brokerAccount.Name,
                State = BrokerAccountStateEnum.Creating,
                CreationDateTime = DateTime.UtcNow,
                TenantId = brokerAccount.TenantId,
                RequestId = requestId
            };

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
                    .FirstOrDefaultAsync(x => x.RequestId == requestId);

                return MapToDomain(entity);
            }
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

            var brokerAccount = BrokerAccount.RestoreAccount(
                brokerAccountEntity.BrokerAccountId,
                brokerAccountEntity.Name,
                brokerAccountEntity.TenantId,
                brokerAccountEntity.CreationDateTime,
                brokerAccountEntity.BlockingDateTime,
                brokerAccountEntity.ActivationDateTime,
                state
            );

            return brokerAccount;
        }
    }
}
