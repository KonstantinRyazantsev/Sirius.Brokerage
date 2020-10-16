using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Assets;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Assets
{
    public interface IAssetsRepository
    {
        Task AddOrReplaceAsync(Asset asset);
        Task<Asset> GetAsync(long id);

        Task<Asset> GetOrDefaultAsync(long id);

        Task<Asset> GetByBlockchainAssetIdAsync(BlockchainAssetId feePayingAssetId);
    }
}
