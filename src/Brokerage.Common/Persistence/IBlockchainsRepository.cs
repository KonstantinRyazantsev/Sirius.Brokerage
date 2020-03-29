using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Blockchains;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public interface IBlockchainsRepository
    {
        Task<Blockchain> AddOrReplaceAsync(Blockchain blockchain);

        Task<IReadOnlyCollection<Blockchain>> GetAllAsync(BlockchainId cursor, int limit);
    }
}
