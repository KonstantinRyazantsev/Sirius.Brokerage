using System.Collections.Generic;
using Brokerage.Bilv1.Domain.Models.Assets;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IAssetService
    {
        IReadOnlyDictionary<(string blockchainId, string networkId), Asset[]> GetAllAssets();
        IReadOnlyCollection<Asset> GetAssetsFor(string blockchainId, string networkId);
        Asset GetAssetForId(string blockchainId, string networkId, string assetId);
        IReadOnlyCollection<Asset> GetAssetsForTicker(string blockchainId, string networkId, string ticker);
    }
}
