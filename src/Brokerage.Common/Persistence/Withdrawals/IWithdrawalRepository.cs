using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;

namespace Brokerage.Common.Persistence.Withdrawals
{
    public interface IWithdrawalRepository
    {
        Task<IReadOnlyCollection<Withdrawal>> GetOrDefaultAsync(
            string transactionId,
            long assetId,
            long brokerAccountRequisitesId);

        Task<Withdrawal> GetAsync(long withdrawalId);
        Task<long> GetNextIdAsync();
        Task<IReadOnlyCollection<Withdrawal>> GetByTransactionIdAsync(string transactionId);
        Task<Withdrawal> GetByOperationIdAsync(long operationId);
        Task SaveAsync(Withdrawal withdrawal);
        Task AddOrIgnoreAsync(Withdrawal withdrawal);
    }
}
