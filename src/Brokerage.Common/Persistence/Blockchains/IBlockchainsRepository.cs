using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Persistence.Blockchains
{
    public interface IBlockchainsRepository
    {
        Task AddOrReplaceAsync(Blockchain blockchain);

        Task<IReadOnlyCollection<Blockchain>> GetAllAsync(string cursor, int limit);
    }
}
