using System.Collections.Generic;
using Brokerage.Bilv1.Domain.Models.Blockchains;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IBlockchainService
    {
        IReadOnlyCollection<Blockchain> GetAllBlockchains();
        Blockchain GetBlockchainById(string blockchainId);
    }
}
