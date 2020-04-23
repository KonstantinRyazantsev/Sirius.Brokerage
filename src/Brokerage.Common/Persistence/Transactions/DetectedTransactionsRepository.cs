using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;

namespace Brokerage.Common.Persistence.Transactions
{
    internal sealed class DetectedTransactionsRepository : IDetectedTransactionsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public DetectedTransactionsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task AddOrIgnore(string blockchainId, string transactionId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            await context.DetectedTransactions.AddAsync(new DetectedTransactionEntity
            {
                BlockchainId = blockchainId,
                TransactionId = transactionId
            });

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        public async Task<bool> Exists(string blockchainId, string transactionId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context.DetectedTransactions
                .FirstOrDefaultAsync(x => x.BlockchainId == blockchainId && x.TransactionId == transactionId);

            return entity != null;
        }

        public async Task DeleteOrIgnore(string blockchainId, string transactionId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            await context.DetectedTransactions
                .Where(x => x.BlockchainId == blockchainId && x.TransactionId == transactionId)
                .DeleteAsync();
        }
    }
}
