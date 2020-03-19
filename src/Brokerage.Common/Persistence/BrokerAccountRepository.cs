using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccount;
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
            string tenantId,
            string blockchainId, 
            string networkId,
            string name)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var newEntity = new BrokerAccountEntity()
            {
                Name = name,
                BlockchainId = blockchainId,
                NetworkId = networkId,
                TenantId = tenantId,
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

            var brokerAccount = new BrokerAccount()
            {
                BlockchainId = brokerAccountEntity.BlockchainId,
                NetworkId = brokerAccountEntity.NetworkId,
                Name = brokerAccountEntity.Name,
                BrokerAccountId = brokerAccountEntity.BrokerAccountId,
                TenantId = brokerAccountEntity.TenantId,
            };

            return brokerAccount;
        }
    }
}
