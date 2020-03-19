using System.Threading.Tasks;
using Brokerage.Common.Domain.Networks;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public class NetworkReadModelRepository : INetworkReadModelRepository
    {
        private readonly DbContextOptionsBuilder<BrokerageContext> _dbContextOptionsBuilder;

        public NetworkReadModelRepository(DbContextOptionsBuilder<BrokerageContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Network> GetOrDefaultAsync(BlockchainId blockchainId, NetworkId networkId)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = await context.Networks.FindAsync(blockchainId.Value, networkId.Value);

            return MapToDomain(entity);
        }

        public async Task<Network> AddOrReplaceAsync(Network network)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = new NetworkEntity()
            {
                BlockchainId = network.BlockchainId,
                NetworkId = network.NetworkId
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

        private static Network MapToDomain(NetworkEntity networkEntity)
        {
            if (networkEntity == null)
                return null;

            return new Network()
            {
                BlockchainId = networkEntity.BlockchainId,
                NetworkId = networkEntity.NetworkId,
            };
        }
    }
}
