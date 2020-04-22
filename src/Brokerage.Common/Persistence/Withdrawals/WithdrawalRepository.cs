using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Brokerage.Common.Persistence.Entities.Withdrawals;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Withdrawals
{
    public class WithdrawalRepository : IWithdrawalRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public WithdrawalRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<IReadOnlyCollection<Withdrawal>> GetOrDefaultAsync(
            string transactionId,
            long assetId,
            long brokerAccountRequisitesId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context
                .Withdrawals
                .Include(x => x.Fees)
                .Where(x => x.TransactionId == transactionId &&
                                          x.AssetId == assetId &&
                                          x.BrokerAccountRequisitesId == brokerAccountRequisitesId);
            await query.LoadAsync();

            return query.AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<Withdrawal> GetAsync(long withdrawalId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .Withdrawals
                .Include(x => x.Fees)
                .FirstAsync(x => x.Id == withdrawalId);

            return MapToDomain(entity);
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.Withdrawals, nameof(WithdrawalEntity.Id));
        }

        public async Task<IReadOnlyCollection<Withdrawal>> GetByTransactionIdAsync(string transactionId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var withdrawalEntities = context
                .Withdrawals
                .Include(x => x.Fees)
                .Where(x => x.TransactionId == transactionId);

            await withdrawalEntities.LoadAsync();

            return withdrawalEntities
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<Withdrawal> GetByOperationIdAsync(long operationId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .Withdrawals
                .Include(x => x.Fees)
                .FirstAsync(x => x.WithdrawalOperationId == operationId);

            return MapToDomain(entity);
        }

        public async Task SaveAsync(Withdrawal withdrawal)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = MapToEntity(withdrawal);

            if (entity.Version == default)
            {
                context.Withdrawals.Add(entity);
            }
            else
            {
                // TODO: Research how to do it better
                context.Withdrawals.Update(entity);
                context.Entry(entity).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
        }

        public async Task AddOrIgnoreAsync(Withdrawal withdrawal)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = MapToEntity(withdrawal);

            context.Withdrawals.Add(entity);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
            }
        }

        private static WithdrawalEntity MapToEntity(Withdrawal withdrawal)
        {
            var depositEntity = new WithdrawalEntity()
            {
                Id = withdrawal.Id,
                Sequence = withdrawal.Sequence,
                Version = withdrawal.Version,
                AssetId = withdrawal.Unit.AssetId,
                Amount = withdrawal.Unit.Amount,
                WithdrawalOperationId = withdrawal.OperationId,
                BrokerAccountRequisitesId = withdrawal.BrokerAccountRequisitesId,
                Fees = withdrawal.Fees?.Select((x) => new WithdrawalFeeEntity()
                {
                    AssetId = x.AssetId,
                    Amount = x.Amount,
                    WithdrawalId = withdrawal.Id
                }).ToArray(),
                WithdrawalErrorMessage = withdrawal.Error?.Message,
                WithdrawalErrorCode = withdrawal.Error?.Code,
                State = withdrawal.State,
                TransactionId = withdrawal.TransactionInfo?.TransactionId,
                TransactionBlock = withdrawal.TransactionInfo?.TransactionBlock,
                TransactionRequiredConfirmationsCount = withdrawal.TransactionInfo?.RequiredConfirmationsCount,
                TransactionDateTime = withdrawal.TransactionInfo?.DateTime,
                BrokerAccountId = withdrawal.BrokerAccountId,
                TenantId = withdrawal.TenantId,
                DestinationTagType = withdrawal.DestinationRequisites.TagType,
                UpdatedDateTime = withdrawal.UpdatedAt,
                CreatedDateTime = withdrawal.CreatedAt,
                DestinationAddress = withdrawal.DestinationRequisites.Address,
                AccountId = withdrawal.AccountId,
                DestinationTag = withdrawal.DestinationRequisites.Tag,
                ReferenceId = withdrawal.ReferenceId
            };

            return depositEntity;
        }

        private static Withdrawal MapToDomain(WithdrawalEntity withdrawalEntity)
        {
            var withdrawal = Withdrawal.Restore(
                withdrawalEntity.Id,
                withdrawalEntity.Version,
                withdrawalEntity.Sequence,
                withdrawalEntity.BrokerAccountId,
                withdrawalEntity.BrokerAccountRequisitesId,
                withdrawalEntity.AccountId,
                withdrawalEntity.ReferenceId,
                new Unit(withdrawalEntity.AssetId, withdrawalEntity.Amount),
                withdrawalEntity.TenantId,
                withdrawalEntity.Fees?
                    .Select(x => new Unit(x.AssetId, x.Amount))
                    .ToArray(),
                new DestinationRequisites(
                    withdrawalEntity.DestinationAddress,
                    withdrawalEntity.DestinationTag,
                    withdrawalEntity.DestinationTagType),
                withdrawalEntity.State,
                withdrawalEntity.TransactionId != null ? new TransactionInfo(
                    withdrawalEntity.TransactionId,
                    // ReSharper disable once PossibleInvalidOperationException
                    withdrawalEntity.TransactionBlock.Value,
                    withdrawalEntity.TransactionRequiredConfirmationsCount.Value,
                    withdrawalEntity.TransactionDateTime.Value.UtcDateTime) : null,
                withdrawalEntity.WithdrawalErrorCode == null ?
                    null :
                    new WithdrawalError(withdrawalEntity.WithdrawalErrorMessage, withdrawalEntity.WithdrawalErrorCode.Value),
                withdrawalEntity.WithdrawalOperationId,
                withdrawalEntity.CreatedDateTime.UtcDateTime,
                withdrawalEntity.UpdatedDateTime.UtcDateTime);

            return withdrawal;
        }
    }
}
