using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Transactions.Operations;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface ITransactionOrderService
    {
        Task<IReadOnlyCollection<TransactionOrder>> GetAllAsync(string blockchainId, string networkId, int skip,
            int take);
    }
}
