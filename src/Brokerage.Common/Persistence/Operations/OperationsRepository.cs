using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Operations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

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
            
            context.Operations.Add(FromDomain(operation));

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx 
                                              && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
            }
        }

        public async Task UpdateAsync(Operation operation)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            context.Operations.Update(FromDomain(operation));

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
            }
        }

        private static Operation ToDomain(OperationEntity entity)
        {
            return Operation.Restore(
                entity.Id, 
                entity.Type, 
                entity.ActualFees.Select(x => new Unit(x.AssetId, x.Amount)).ToArray(),
                entity.ExpectedFees.Select(x => new Unit(x.AssetId, x.Amount)).ToArray(),
                entity.Version);
        }

        private static OperationEntity FromDomain(Operation operation)
        {
            return new OperationEntity
            {
                Id = operation.Id,
                Type = operation.Type,
                ExpectedFees = operation?.ExpectedFees.Select(x => new ExpectedOperationFeeEntity()
                {
                    Amount = x.Amount,
                    AssetId = x.AssetId
                }).ToArray(),
                ActualFees = operation?.ActualFees.Select(x => new ActualOperationFeeEntity()
                {
                    Amount = x.Amount,
                    AssetId = x.AssetId
                }).ToArray(),
                Version = operation.Version
            };
        }
    }
}
