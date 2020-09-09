using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Persistence.Deposits
{
    public interface IDepositsRepository
    {
        Task Save(IReadOnlyCollection<Deposit> deposits);
        Task<IReadOnlyCollection<Deposit>> Search(string blockchainId, string transactionId, long? consolidationOperationId);
        Task<IReadOnlyCollection<Deposit>> GetAnyFor(HashSet<long> toHashSet);
    }
}
