using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Brokerage.Common.Persistence.Entities.Deposits;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.Deposits
{
    public class DepositsRepository : IDepositsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public DepositsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<Deposit> GetOrDefaultAsync(
            string transactionId,
            long assetId,
            long brokerAccountRequisitesId,
            long? accountRequisitesId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .Deposits
                .Include(x => x.Sources)
                .Include(x => x.Fees)
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId &&
                                          x.AssetId == assetId &&
                                          x.BrokerAccountRequisitesId == brokerAccountRequisitesId && 
                                          x.AccountRequisitesId == accountRequisitesId);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.Deposits, nameof(DepositEntity.Id));
        }

        public async Task SaveAsync(Deposit deposit)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = MapToEntity(deposit);

            if (entity.Version == default)
            {
                context.Deposits.Add(entity);
            }
            else
            {
                // TODO: Research how to do it better
                context.Deposits.Update(entity);
                context.Entry(entity).State = EntityState.Modified;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static DepositEntity MapToEntity(Deposit deposit)
        {
            var depositState = deposit.DepositState switch
            {
                DepositState.Detected =>  DepositStateEnum.Detected,
                DepositState.Cancelled => DepositStateEnum.Cancelled,
                DepositState.Completed => DepositStateEnum.Completed,
                DepositState.Confirmed => DepositStateEnum.Confirmed,
                DepositState.Failed =>    DepositStateEnum.Failed,

                _ => throw new ArgumentOutOfRangeException(nameof(deposit.DepositState),
                    deposit.DepositState,
                    null)
            };

            var errorCode = deposit.Error?.Code == null ? (DepositErrorCodeEnum?)null : deposit.Error.Code switch
            {
                DepositError.DepositErrorCode.TechnicalProblem => DepositErrorCodeEnum.TechnicalProblem,
                _ => throw new ArgumentOutOfRangeException(nameof(deposit.Error.Code), deposit.Error?.Code, null)
            };

            var depositEntity = new DepositEntity()
            {
                Id = deposit.Id,
                Sequence = deposit.Sequence,
                Version = deposit.Version,
                AssetId = deposit.AssetId,
                Amount = deposit.Amount,
                BrokerAccountRequisitesId = deposit.BrokerAccountRequisitesId,
                Fees = deposit.Fees?.Select((x, index) => new DepositFeeEntity()
                {
                    AssetId = x.AssetId,
                    Amount = x.Amount,
                    DepositId = deposit.Id
                }).ToArray(),
                ErrorMessage = deposit.Error?.Message,
                ErrorCode = errorCode,
                Sources = deposit.Sources.Select((x, index) => new DepositSourceEntity()
                {
                    Address = x.Address,
                    Amount = x.Amount,
                    DepositId = deposit.Id
                }).ToArray(),
                AccountRequisitesId = deposit.AccountRequisitesId,
                DepositState = depositState,
                TransactionId = deposit.TransactionInfo.TransactionId,
                TransactionBlock = deposit.TransactionInfo.TransactionBlock,
                TransactionRequiredConfirmationsCount = deposit.TransactionInfo.RequiredConfirmationsCount,
                TransactionDateTime = deposit.TransactionInfo.DateTime,
                DetectedDateTime = deposit.DetectedDateTime,
                ConfirmedDateTime = deposit.ConfirmedDateTime,
                CompletedDateTime = deposit.CompletedDateTime,
                CancelledDateTime = deposit.CancelledDateTime,
                FailedDateTime = deposit.FailedDateTime,
            };

            return depositEntity;
        }

        private static Deposit MapToDomain(DepositEntity depositEntity)
        {
            var depositError = depositEntity.ErrorMessage == null && depositEntity.ErrorCode == null
                ? null
                : new
                    DepositError(depositEntity.ErrorMessage,
                        (depositEntity.ErrorCode.Value switch
                        {
                            DepositErrorCodeEnum.TechnicalProblem => DepositError.DepositErrorCode.TechnicalProblem,
                            _ => throw new ArgumentOutOfRangeException(nameof(depositEntity.ErrorCode),
                                depositEntity.ErrorCode,
                                null)
                        }));

            var depositState = depositEntity.DepositState switch
            {
                DepositStateEnum.Detected => DepositState.Detected,
                DepositStateEnum.Cancelled => DepositState.Cancelled,
                DepositStateEnum.Completed => DepositState.Completed,
                DepositStateEnum.Confirmed => DepositState.Confirmed,
                DepositStateEnum.Failed => DepositState.Failed,

                _ => throw new ArgumentOutOfRangeException(nameof(depositEntity.DepositState),
                    depositEntity.DepositState,
                    null)
            };

            var deposit = Deposit.Restore(
                depositEntity.Id,
                depositEntity.Version,
                depositEntity.Sequence,
                depositEntity.BrokerAccountRequisitesId,
                depositEntity.AccountRequisitesId,
                depositEntity.AssetId,
                depositEntity.Amount,
                depositEntity.Fees?
                    .Select(x => new Fee(x.AssetId, x.Amount))
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
                depositEntity.DetectedDateTime.UtcDateTime,
                depositEntity.ConfirmedDateTime?.UtcDateTime,
                depositEntity.CompletedDateTime?.UtcDateTime,
                depositEntity.FailedDateTime?.UtcDateTime,
                depositEntity.CancelledDateTime?.UtcDateTime);

                return deposit;
        }
    }
}
