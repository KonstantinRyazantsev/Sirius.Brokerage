using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.Operations
{
    internal sealed class OperationsRepository : IOperationsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public OperationsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Operation> GetOrDefault(long id)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context.Operations
                .FirstAsync(x => x.Id == id);

            return ToDomain(entity);
        }

        public async Task AddOrIgnore(Operation operation)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            await context.Operations.AddAsync(FromDomain(operation));

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        private static Operation ToDomain(OperationEntity entity)
        {
            return new Operation(entity.Id, entity.Type);
        }

        private static OperationEntity FromDomain(Operation operation)
        {
            return new OperationEntity
            {
                Id = operation.Id,
                Type = operation.Type
            };
        }
    }
}
