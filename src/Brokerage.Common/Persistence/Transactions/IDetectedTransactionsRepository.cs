using System.Threading.Tasks;

namespace Brokerage.Common.Persistence.Transactions
{
    public interface IDetectedTransactionsRepository
    {
        Task Add(string blockchainId, string transactionId);
        Task<bool> Exists(string blockchainId, string transactionId);
    }
}
