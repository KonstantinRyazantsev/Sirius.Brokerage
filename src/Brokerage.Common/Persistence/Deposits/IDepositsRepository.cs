using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Persistence.Deposits
{
    public interface IDepositsRepository
    {
        Task<Deposit> GetOrDefaultAsync(
            string transactionId, 
            long assetId,
            long brokerAccountRequisitesId,
            long? accountRequisitesId);

        Task<long> GetNextIdAsync();
        Task SaveAsync(Deposit deposit);

        Task<IReadOnlyCollection<Deposit>> GetByTransactionIdAsync(string transactionId);
        Task<Deposit> GetByOperationIdOrDefaultAsync(long operationId);
    }
}
