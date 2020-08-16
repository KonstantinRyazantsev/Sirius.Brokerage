using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Microsoft.EntityFrameworkCore;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Operations
{
    internal sealed class OperationsRepository : IOperationsRepository
    {
        private readonly DatabaseContext _dbContext;

        public OperationsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Operation> GetOrDefault(long id)
        {
            var entity = await _dbContext.Operations
                .FirstAsync(x => x.Id == id);

            return ToDomain(entity);
        }

        public async Task Add(Operation operation)
        {
            _dbContext.Operations.Add(FromDomain(operation));

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Operation operation)
        {
            _dbContext.Operations.Update(FromDomain(operation));

            await _dbContext.SaveChangesAsync();
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
