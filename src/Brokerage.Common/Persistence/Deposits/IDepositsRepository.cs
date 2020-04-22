using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Persistence.Deposits
{
    public interface IDepositsRepository
    {
        Task<long> GetNextIdAsync();
        Task SaveAsync(IReadOnlyCollection<Deposit> deposits);
        Task<IReadOnlyCollection<Deposit>> GetAllByTransactionAsync(string blockchainId, string transactionId);
        Task<Deposit> GetByonsolidationIdOrDefaultAsync(long operationId);
    }
}
