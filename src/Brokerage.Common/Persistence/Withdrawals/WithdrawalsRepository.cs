using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Withdrawals;
using Microsoft.EntityFrameworkCore;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Withdrawals
{
    public class WithdrawalsRepository : IWithdrawalsRepository
    {
        private readonly DatabaseContext _dbContext;

        public WithdrawalsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Withdrawal> Get(long withdrawalId)
        {
            var entity = await _dbContext
                .Withdrawals
                .FirstAsync(x => x.Id == withdrawalId);

            return MapToDomain(entity);
        }

        public async Task<Withdrawal> GetByOperationIdOrDefault(long operationId)
        {
            var entity = await _dbContext
                .Withdrawals
                .FirstOrDefaultAsync(x => x.OperationId == operationId);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task Update(IReadOnlyCollection<Withdrawal> withdrawals)
        {
            if (!withdrawals.Any())
            {
                return;
            }

            foreach (var withdrawal in withdrawals)
            {
                var entity = MapToEntity(withdrawal);

                _dbContext.Withdrawals.Update(entity);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Add(Withdrawal withdrawal)
        {
            var entity = MapToEntity(withdrawal);

            _dbContext.Withdrawals.Add(entity);

            await _dbContext.SaveChangesAsync();
        }

        private static WithdrawalEntity MapToEntity(Withdrawal withdrawal)
        {
            var depositEntity = new WithdrawalEntity
            {
                Id = withdrawal.Id,
                TenantId = withdrawal.TenantId,
                BrokerAccountId = withdrawal.BrokerAccountId,
                BrokerAccountDetailsId = withdrawal.BrokerAccountDetailsId,
                AccountId = withdrawal.AccountId,
                AssetId = withdrawal.Unit.AssetId,
                Amount = withdrawal.Unit.Amount,
                Fees = withdrawal.Fees,
                DestinationAddress = withdrawal.DestinationDetails.Address,
                DestinationTag = withdrawal.DestinationDetails.Tag,
                DestinationTagType = withdrawal.DestinationDetails.TagType,
                State = withdrawal.State,
                TransactionId = withdrawal.TransactionInfo?.TransactionId,
                TransactionBlock = withdrawal.TransactionInfo?.TransactionBlock,
                TransactionRequiredConfirmationsCount = withdrawal.TransactionInfo?.RequiredConfirmationsCount,
                TransactionDateTime = withdrawal.TransactionInfo?.DateTime,
                WithdrawalErrorMessage = withdrawal.Error?.Message,
                WithdrawalErrorCode = withdrawal.Error?.Code,
                TransferContext = withdrawal.TransferContext,
                OperationId = withdrawal.OperationId,
                Version = withdrawal.Version,
                Sequence = withdrawal.Sequence,
                UpdatedAt = withdrawal.UpdatedAt,
                CreatedAt = withdrawal.CreatedAt
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
                withdrawalEntity.BrokerAccountDetailsId,
                withdrawalEntity.AccountId,
                new Unit(withdrawalEntity.AssetId, withdrawalEntity.Amount),
                withdrawalEntity.TenantId,
                withdrawalEntity.Fees,
                new DestinationDetails(
                    withdrawalEntity.DestinationAddress,
                    withdrawalEntity.DestinationTag,
                    withdrawalEntity.DestinationTagType),
                withdrawalEntity.State,
                withdrawalEntity.TransactionId != null
                    ? new TransactionInfo(
                        withdrawalEntity.TransactionId,
                        // ReSharper disable once PossibleInvalidOperationException
                        withdrawalEntity.TransactionBlock.Value,
                        // ReSharper disable once PossibleInvalidOperationException
                        withdrawalEntity.TransactionRequiredConfirmationsCount.Value,
                        // ReSharper disable once PossibleInvalidOperationException
                        withdrawalEntity.TransactionDateTime.Value.UtcDateTime)
                    : null,
                withdrawalEntity.WithdrawalErrorCode != null
                    ? new WithdrawalError(
                        withdrawalEntity.WithdrawalErrorMessage,
                        withdrawalEntity.WithdrawalErrorCode.Value)
                    : null,
                withdrawalEntity.OperationId,
                withdrawalEntity.CreatedAt.UtcDateTime,
                withdrawalEntity.UpdatedAt.UtcDateTime,
                withdrawalEntity.TransferContext);

            return withdrawal;
        }
    }
}
