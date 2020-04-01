using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;

namespace Brokerage.Bilv1.Domain.Repositories
{
    public interface IEnrolledBalanceRepository
    {
        Task<IReadOnlyCollection<EnrolledBalance>> GetAsync(IEnumerable<DepositWalletKey> keys);

        Task SetBalanceAsync(DepositWalletKey key, decimal balance, long balanceBlock);

        Task ResetBalanceAsync(DepositWalletKey key, long transactionBlock);

        Task DeleteBalanceAsync(DepositWalletKey key);

        Task<EnrolledBalance> TryGetAsync(DepositWalletKey key);

        Task<IReadOnlyCollection<EnrolledBalance>> GetAllAsync(int skip, int count);

        Task<IReadOnlyCollection<EnrolledBalance>> GetAllForBlockchainAsync(string blockchainId, int skip, int count);
    }
}
