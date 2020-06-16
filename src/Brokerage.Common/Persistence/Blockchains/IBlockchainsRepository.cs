using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Persistence.Blockchains
{
    public interface IBlockchainsRepository
    {
        Task<long> GetCountAsync();

        Task AddOrReplaceAsync(Blockchain blockchain);

        Task<IReadOnlyCollection<Blockchain>> GetAllAsync(string cursor, int limit);

        Task<Blockchain> GetAsync(string blockchainId);

        Task<Blockchain> GetOrDefaultAsync(string blockchainId);

        Task Add(Blockchain blockchain);

        Task Update(Blockchain blockchain);
    }
}
