using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Assets;

namespace Brokerage.Common.Persistence.Assets
{
    public interface IAssetsRepository
    {
        Task AddOrReplaceAsync(Asset asset);
        Task<Asset> GetAsync(long id);

        Task<Asset> GetOrDefaultAsync(long id);
    }
}
