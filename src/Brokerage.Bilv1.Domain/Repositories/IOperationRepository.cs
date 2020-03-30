using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;
using Brokerage.Bilv1.Domain.Models.Operations;

namespace Brokerage.Bilv1.Domain.Repositories
{
    public interface IOperationRepository
    {
        Task<Operation> AddAsync(DepositWalletKey key, BigInteger balanceChange, long block);

        Task<IEnumerable<Operation>> GetAsync(DepositWalletKey key, int skip, int take);

        Task<IEnumerable<Operation>> GetAsync(string blockchainId, string walletAddress, int skip, int take);

        Task<IEnumerable<Operation>> GetAllForBlockchainAsync(string blockchainId, int skip, int take);
    }
}
