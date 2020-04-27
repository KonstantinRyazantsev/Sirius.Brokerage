using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Deposits;
using Microsoft.EntityFrameworkCore;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Deposits
{
    public class DepositsRepository : IDepositsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public DepositsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.Deposits, nameof(DepositEntity.Id));
        }

        public async Task<IReadOnlyCollection<Deposit>> GetAllByTransactionAsync(string blockchainId, string transactionId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var deposits = context
                .Deposits
                .Include(x => x.Sources)
                .Include(x => x.Fees)
                .Where(x => x.BlockchainId == blockchainId && x.TransactionId == transactionId);

            await deposits.LoadAsync();

            return deposits
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<Deposit> GetByConsolidationIdOrDefaultAsync(long operationId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .Deposits
                .Include(x => x.Sources)
                .Include(x => x.Fees)
                .FirstOrDefaultAsync(x => x.ConsolidationOperationId == operationId);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task SaveAsync(IReadOnlyCollection<Deposit> deposits)
        {
            if (!deposits.Any())
            {
                return;
            }

            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entities = deposits.Select(MapToEntity);

            foreach (var entity in entities)
            {
                if (entity.Version == default)
                {
                    context.Deposits.Add(entity);
                }
                else
                {
                    context.Deposits.Update(entity);
                    context.Entry(entity).State = EntityState.Modified;
                }    
            }

            await context.SaveChangesAsync();
        }

        private static DepositEntity MapToEntity(Deposit deposit)
        {
            var depositState = deposit.State switch
            {
                DepositState.Detected =>  DepositStateEnum.Detected,
                DepositState.Cancelled => DepositStateEnum.Cancelled,
                DepositState.Completed => DepositStateEnum.Completed,
                DepositState.Confirmed => DepositStateEnum.Confirmed,
                DepositState.Failed =>    DepositStateEnum.Failed,

                _ => throw new ArgumentOutOfRangeException(nameof(deposit.State),
                    deposit.State,
                    null)
            };

            var depositEntity = new DepositEntity
            {
                Id = deposit.Id,
                Sequence = deposit.Sequence,
                Version = deposit.Version,
                TenantId = deposit.TenantId,
                BlockchainId = deposit.BlockchainId,
                BrokerAccountId = deposit.BrokerAccountId,
                AssetId = deposit.Unit.AssetId,
                Amount = deposit.Unit.Amount,
                ConsolidationOperationId = deposit.ConsolidationOperationId,
                BrokerAccountRequisitesId = deposit.BrokerAccountRequisitesId,
                Fees = deposit.Fees?.Select((x, index) => new DepositFeeEntity
                {
                    AssetId = x.AssetId,
                    Amount = x.Amount,
                    DepositId = deposit.Id
                }).ToArray(),
                ErrorMessage = deposit.Error?.Message,
                ErrorCode = deposit.Error?.Code,
                Sources = deposit.Sources.Select((x, index) => new DepositSourceEntity
                {
                    Address = x.Address,
                    Amount = x.Amount,
                    DepositId = deposit.Id
                }).ToArray(),
                AccountRequisitesId = deposit.AccountRequisitesId,
                State = depositState,
                TransactionId = deposit.TransactionInfo.TransactionId,
                TransactionBlock = deposit.TransactionInfo.TransactionBlock,
                TransactionRequiredConfirmationsCount = deposit.TransactionInfo.RequiredConfirmationsCount,
                TransactionDateTime = deposit.TransactionInfo.DateTime,
                CreatedAt = deposit.CreatedAt,
                UpdatedAt = deposit.UpdatedAt,
            };

            return depositEntity;
        }

        private static Deposit MapToDomain(DepositEntity depositEntity)
        {
            var depositError = depositEntity.ErrorMessage == null && depositEntity.ErrorCode == null
                ? null
                : new DepositError(depositEntity.ErrorMessage, depositEntity.ErrorCode ?? DepositErrorCode.TechnicalProblem);

            var depositState = depositEntity.State switch
            {
                DepositStateEnum.Detected => DepositState.Detected,
                DepositStateEnum.Cancelled => DepositState.Cancelled,
                DepositStateEnum.Completed => DepositState.Completed,
                DepositStateEnum.Confirmed => DepositState.Confirmed,
                DepositStateEnum.Failed => DepositState.Failed,

                _ => throw new ArgumentOutOfRangeException(nameof(depositEntity.State),
                    depositEntity.State,
                    null)
            };

            var deposit = Deposit.Restore(
                depositEntity.Id,
                depositEntity.Version,
                depositEntity.Sequence,
                depositEntity.TenantId,
                depositEntity.BlockchainId,
                depositEntity.BrokerAccountId,
                depositEntity.BrokerAccountRequisitesId,
                depositEntity.AccountRequisitesId,
                new Unit(depositEntity.AssetId, depositEntity.Amount),
                depositEntity.ConsolidationOperationId,
                depositEntity.Fees?
                    .Select(x => new Unit(x.AssetId, x.Amount))
                    .ToArray(),
                new TransactionInfo(
                    depositEntity.TransactionId,
                    depositEntity.TransactionBlock,
                    depositEntity.TransactionRequiredConfirmationsCount,
                    depositEntity.TransactionDateTime),
                depositError,
                depositState,
                depositEntity.Sources?
                    .Select(x => new DepositSource(x.Address, x.Amount))
                    .ToArray(),
                depositEntity.CreatedAt.UtcDateTime,
                depositEntity.UpdatedAt.UtcDateTime);

            return deposit;
        }
    }
}
