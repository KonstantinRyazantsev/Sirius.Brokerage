using System.Collections.Generic;
using Brokerage.Bilv1.Domain.Models.Networks;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface INetworkService
    {
        IReadOnlyDictionary<string, Network[]> GetAllNetworks();
        IReadOnlyCollection<Network> GetNetworksByBlockchainType(string blockchainType);
        Network GetNetworkById(string networkId);
    }
}
