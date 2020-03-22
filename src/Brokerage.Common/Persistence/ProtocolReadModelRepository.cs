using System.Threading.Tasks;
using Brokerage.Common.Domain.Protocols;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public class ProtocolReadModelRepository : IProtocolReadModelRepository
    {
        private readonly DbContextOptionsBuilder<BrokerageContext> _dbContextOptionsBuilder;

        public ProtocolReadModelRepository(DbContextOptionsBuilder<BrokerageContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Protocol> GetOrDefaultAsync(ProtocolId protocolId)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = await context.Networks.FindAsync(protocolId.Value);

            return MapToDomain(entity);
        }

        public async Task<Protocol> AddOrReplaceAsync(Protocol protocol)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = new ProtocolEntity()
            {
                ProtocolId = protocol.ProtocolId
            };

            try
            {
                context.Networks.Add(entity);
                await context.SaveChangesAsync();

                return MapToDomain(entity);
            }
            catch (DbUpdateException e) //Check that request was already processed (by constraint)
                when (e.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == "23505")
            {
                context.Networks.Update(entity);

                await context.SaveChangesAsync();

                return MapToDomain(entity);
            }
        }

        private static Protocol MapToDomain(ProtocolEntity protocolEntity)
        {
            if (protocolEntity == null)
                return null;

            return new Protocol()
            {
                ProtocolId = protocolEntity.ProtocolId
            };
        }
    }
}
