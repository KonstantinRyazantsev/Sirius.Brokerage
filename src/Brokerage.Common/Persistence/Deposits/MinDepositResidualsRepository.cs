using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.Deposits;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.Deposits
{
    public class MinDepositResidualsRepository : IMinDepositResidualsRepository
    {
        private readonly DatabaseContext _dbContext;

        public MinDepositResidualsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<MinDepositResidual>> GetAnyOfForUpdate(ISet<AccountDetailsId> ids)
        {
            if (!ids.Any())
                return Array.Empty<MinDepositResidual>();

            var idStrings = ids.Select(x => x.ToString()).ToArray();

            var addressesInList = string.Join(", ", idStrings.Select(x => $"('{x}')"));
            var selectQuery = $@"
                    select *, 
                    {nameof(MinDepositResidualEntity.xmin)} 
                    from {DatabaseContext.SchemaName}.{Tables.MinDepositResiduals}
                    where ""{nameof(MinDepositResidualEntity.ConsolidationDepositId)}"" is null and 
                    ""{nameof(MinDepositResidualEntity.NaturalId)}"" in (values {addressesInList})
                    order by ""{nameof(MinDepositResidualEntity.NaturalId)}""
                    for update;";
            var lockedEntities = await _dbContext.Database.GetDbConnection()
                    .QueryAsync<MinDepositResidualEntity>(selectQuery);

            return lockedEntities
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task Save(IReadOnlyCollection<MinDepositResidual> newMinDepositResiduals)
        {
            if (!newMinDepositResiduals.Any())
            {
                return;
            }

            var entities = newMinDepositResiduals.Select(MapToEntity).ToArray();

            foreach (var entity in entities)
            {
                if (entity.xmin == default)
                {
                    _dbContext.MinDepositResiduals.Add(entity);
                }
                else
                {
                    _dbContext.MinDepositResiduals.Update(entity);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Remove(IReadOnlyCollection<MinDepositResidual> prevMinDepositResiduals)
        {
            if (!prevMinDepositResiduals.Any())
            {
                return;
            }

            var entities = prevMinDepositResiduals.Select(MapToEntity).ToArray();

            _dbContext.MinDepositResiduals.RemoveRange(entities);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<MinDepositResidual>> GetForConsolidationDeposits(HashSet<long> consolidationDeposits)
        {
            if (!consolidationDeposits.Any())
                return Array.Empty<MinDepositResidual>();

            var minDepositResiduals = _dbContext.MinDepositResiduals.AsQueryable();

            await minDepositResiduals
                .Where(x => x.ConsolidationDepositId != null && 
                            consolidationDeposits.Contains(x.ConsolidationDepositId.Value))
                .LoadAsync();

            return minDepositResiduals
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        private MinDepositResidual MapToDomain(MinDepositResidualEntity entity)
        {
            var domainModel = MinDepositResidual.Restore(
                entity.DepositId,
                entity.Amount,
                new AccountDetailsId(entity.BlockchainId,
                    entity.Address,
                    entity.Tag,
                    entity.TagType),
                entity.AssetId,
                entity.ConsolidationDepositId,
                entity.CreatedAt,
                entity.xmin);

            return domainModel;
        }

        private MinDepositResidualEntity MapToEntity(MinDepositResidual model)
        {
            var entity = new MinDepositResidualEntity()
            {
                AssetId = model.AssetId,
                Amount = model.Amount,
                DepositId = model.DepositId,
                BlockchainId = model.AccountDetailsId.BlockchainId,
                Address = model.AccountDetailsId.Address,
                ConsolidationDepositId = model.ConsolidationDepositId,
                Tag = model.AccountDetailsId.Tag,
                CreatedAt = model.CreatedAt,
                TagType = model.AccountDetailsId.TagType,
                NaturalId = model.AccountDetailsId.ToString(),
                xmin = model.Version
            };

            return entity;
        }
    }
}
