using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;

namespace Brokerage.Common.Persistence.Withdrawals
{
    public interface IWithdrawalRepository
    {
        Task<Withdrawal> GetAsync(long withdrawalId);
        Task<long> GetNextIdAsync();
        Task<Withdrawal> GetByOperationIdOrDefaultAsync(long operationId);
        Task SaveAsync(IReadOnlyCollection<Withdrawal> withdrawals);
        Task AddOrIgnoreAsync(Withdrawal withdrawal);
    }
}
