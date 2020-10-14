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
        private readonly DatabaseContext _dbContext;
        private readonly IDepositFactory _depositFactory;

        public DepositsRepository(DatabaseContext dbContext, IDepositFactory depositFactory)
        {
            _dbContext = dbContext;
            _depositFactory = depositFactory;
        }

        public async Task<IReadOnlyCollection<Deposit>> Search(string blockchainId, 
            string transactionId,
            long? consolidationOperationId)
        {
            var deposits = _dbContext
                .Deposits
                .AsQueryable();

            if (blockchainId != null)
            {
                deposits = deposits.Where(x => x.BlockchainId == blockchainId);
            }

            if (transactionId != null)
            {
                if (consolidationOperationId != null)
                {
                    deposits = deposits.Where(x => x.TransactionId == transactionId || x.ConsolidationOperationId == consolidationOperationId);
                }
                else
                {
                    deposits = deposits.Where(x => x.TransactionId == transactionId);
                }
            }
            else
            {
                if (consolidationOperationId != null)
                {
                    deposits = deposits.Where(x => x.ConsolidationOperationId == consolidationOperationId);
                }
            }

            await deposits.LoadAsync();

            return deposits
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyCollection<Deposit>> GetAnyFor(HashSet<long> consolidationDepositsIds)
        {
            if (!consolidationDepositsIds.Any())
                return Array.Empty<Deposit>();

            var deposits = _dbContext
                .Deposits
                .AsQueryable();

            await deposits.Where(x => consolidationDepositsIds.Contains(x.Id)).LoadAsync();

            return deposits
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task Save(IReadOnlyCollection<Deposit> deposits)
        {
            if (!deposits.Any())
            {
                return;
            }

            var entities = deposits.Select(MapToEntity);

            foreach (var entity in entities)
            {
                if (entity.Version == default)
                {
                    _dbContext.Deposits.Add(entity);
                }
                else
                {
                    _dbContext.Deposits.Update(entity);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private DepositEntity MapToEntity(Deposit deposit)
        {
            var depositState = deposit.State switch
            {
                DepositState.Detected =>  DepositStateEnum.Detected,
                DepositState.Cancelled => DepositStateEnum.Cancelled,
                DepositState.Completed => DepositStateEnum.Completed,
                DepositState.Confirmed => DepositStateEnum.Confirmed,
                DepositState.Failed =>    DepositStateEnum.Failed,
                DepositState.ConfirmedTiny => DepositStateEnum.ConfirmedTiny,
                DepositState.DetectedTiny => DepositStateEnum.DetectedTiny,
                DepositState.CompletedTiny => DepositStateEnum.CompletedTiny,

                _ => throw new ArgumentOutOfRangeException(nameof(deposit.State),
                    deposit.State,
                    null)
            };

            var depositType = deposit.DepositType switch
            {
                DepositType.TinyDeposit => DepositTypeEnum.TinyDeposit,
                DepositType.BrokerDeposit => DepositTypeEnum.BrokerDeposit,
                DepositType.RegularDeposit => DepositTypeEnum.RegularDeposit,
                DepositType.TokenDeposit => DepositTypeEnum.TokenDeposit,

                _ => throw new ArgumentOutOfRangeException(nameof(deposit.DepositType), deposit.DepositType, null)
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
                BrokerAccountDetailsId = deposit.BrokerAccountDetailsId,
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
                AccountDetailsId = deposit.AccountDetailsId,
                State = depositState,
                TransactionId = deposit.TransactionInfo.TransactionId,
                TransactionBlock = deposit.TransactionInfo.TransactionBlock,
                TransactionRequiredConfirmationsCount = deposit.TransactionInfo.RequiredConfirmationsCount,
                TransactionDateTime = deposit.TransactionInfo.DateTime,
                CreatedAt = deposit.CreatedAt,
                UpdatedAt = deposit.UpdatedAt,
                MinDepositForConsolidation = deposit.MinDepositForConsolidation,
                DepositType = depositType
            };

            return depositEntity;
        }

        private Deposit MapToDomain(DepositEntity depositEntity)
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
                DepositStateEnum.ConfirmedTiny => DepositState.ConfirmedTiny,
                DepositStateEnum.DetectedTiny => DepositState.DetectedTiny,
                DepositStateEnum.CompletedTiny => DepositState.CompletedTiny,

                _ => throw new ArgumentOutOfRangeException(nameof(depositEntity.State),
                    depositEntity.State,
                    null)
            };

            var depositType = depositEntity.DepositType switch
            {
                DepositTypeEnum.TinyDeposit => DepositType.TinyDeposit,
                DepositTypeEnum.BrokerDeposit => DepositType.BrokerDeposit,
                DepositTypeEnum.RegularDeposit => DepositType.RegularDeposit,
                DepositTypeEnum.TokenDeposit => DepositType.TokenDeposit,

                _ => throw new ArgumentOutOfRangeException(nameof(depositEntity.DepositType), depositEntity.DepositType, null)
            };

            var deposit = _depositFactory.Restore(
                depositEntity.Id,
                depositEntity.Version,
                depositEntity.Sequence,
                depositEntity.TenantId,
                depositEntity.BlockchainId,
                depositEntity.BrokerAccountId,
                depositEntity.BrokerAccountDetailsId,
                depositEntity.AccountDetailsId,
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
                depositEntity.UpdatedAt.UtcDateTime,
                depositEntity.MinDepositForConsolidation,
                depositType);

            return deposit;
        }
    }
}
