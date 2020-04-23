using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Assets;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.Assets
{
    public sealed class AssetsRepository : IAssetsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public AssetsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task AddOrReplaceAsync(Asset asset)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            try
            {
                context.Assets.Add(asset);

                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                context.Assets.Update(asset);

                await context.SaveChangesAsync();
            }
        }

        public async Task<Asset> GetAsync(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var asset = await context.Assets.FirstAsync(x => x.Id == id);

            return asset;
        }

        public async Task<Asset> GetOrDefaultAsync(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var asset = await context.Assets.FirstOrDefaultAsync(x => x.Id == id);

            return asset;
        }
    }
}
