using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;

namespace Brokerage.Common.Persistence.Withdrawals
{
    public interface IWithdrawalsRepository
    {
        Task<Withdrawal> Get(long withdrawalId);
        Task<Withdrawal> GetByOperationIdOrDefault(long operationId);
        Task Update(IReadOnlyCollection<Withdrawal> withdrawals);
        Task Add(Withdrawal withdrawal);
    }
}
