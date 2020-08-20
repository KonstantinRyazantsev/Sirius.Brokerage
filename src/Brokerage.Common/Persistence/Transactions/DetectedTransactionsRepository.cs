using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.Transactions
{
    internal sealed class DetectedTransactionsRepository : IDetectedTransactionsRepository
    {
        private readonly DatabaseContext _dbContext;

        public DetectedTransactionsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(string blockchainId, string transactionId)
        {
            _dbContext.DetectedTransactions.Add(new DetectedTransactionEntity
            {
                BlockchainId = blockchainId,
                TransactionId = transactionId
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Exists(string blockchainId, string transactionId)
        {
            var entity = await _dbContext.DetectedTransactions
                .FirstOrDefaultAsync(x => x.BlockchainId == blockchainId && x.TransactionId == transactionId);

            return entity != null;
        }
    }
}
