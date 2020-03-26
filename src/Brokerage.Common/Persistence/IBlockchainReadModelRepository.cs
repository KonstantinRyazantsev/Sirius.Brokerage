using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Blockchains;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public interface IBlockchainReadModelRepository
    {
        Task<Blockchain> GetOrDefaultAsync(BlockchainId blockchainId);

        Task<Blockchain> AddOrReplaceAsync(Blockchain blockchain);

        Task<IReadOnlyCollection<Blockchain>> GetManyAsync(BlockchainId cursor, int take);
    }
}
