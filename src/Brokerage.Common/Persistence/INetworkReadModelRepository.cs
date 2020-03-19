using System.Threading.Tasks;
using Brokerage.Common.Domain.Networks;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public interface INetworkReadModelRepository
    {
        Task<Network> GetOrDefaultAsync(BlockchainId blockchainId, NetworkId networkId);

        Task<Network> AddOrReplaceAsync(Network network);
    }
}
