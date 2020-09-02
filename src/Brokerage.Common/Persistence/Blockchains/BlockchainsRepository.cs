using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Blockchains;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;

namespace Brokerage.Common.Persistence.Blockchains
{
    public class BlockchainsRepository : IBlockchainsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public BlockchainsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<IReadOnlyCollection<Blockchain>> GetAllAsync(string cursor, int limit)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context.Blockchains.Select(x => x);

            if (cursor != null)
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                query = query.Where(x => x.Id.CompareTo(cursor) > 1);
            }

            return await query
                .OrderBy(x => x.Id)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<long> GetCountAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var count = await context.Blockchains.CountAsync();

            return count;
        }

        public async Task AddOrReplaceAsync(Blockchain blockchain)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var affectedRowsCount = await context.Blockchains
                .Where(x => x.Id == blockchain.Id &&
                            x.UpdatedAt <= blockchain.UpdatedAt)
                .UpdateAsync(x => new Blockchain
                {
                    NetworkType = blockchain.NetworkType,
                    UpdatedAt = blockchain.UpdatedAt,
                    Protocol = blockchain.Protocol,
                    CreatedAt = blockchain.CreatedAt,
                    Id = blockchain.Id
                });

            if (affectedRowsCount == 0)
            {
                try
                {
                    context.Blockchains.Add(blockchain);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx
                                                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    //Swallow error: the entity was already added
                }
            }
        }

        public async Task<Blockchain> GetAsync(string blockchainId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var blockchain = await context.Blockchains.FirstAsync(x => x.Id == blockchainId);

            return blockchain;
        }

        public async Task<Blockchain> GetOrDefaultAsync(string blockchainId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var blockchain = await context.Blockchains.FirstOrDefaultAsync(x => x.Id == blockchainId);

            return blockchain;
        }
    }
}
