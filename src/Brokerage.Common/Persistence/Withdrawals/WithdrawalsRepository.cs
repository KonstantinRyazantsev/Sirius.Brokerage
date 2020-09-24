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
            var depositEntity = new WithdrawalEntity()
            {
                Id = withdrawal.Id,
                Sequence = withdrawal.Sequence,
                Version = withdrawal.Version,
                AssetId = withdrawal.Unit.AssetId,
                Amount = withdrawal.Unit.Amount,
                OperationId = withdrawal.OperationId,
                BrokerAccountDetailsId = withdrawal.BrokerAccountDetailsId,
                Fees = withdrawal.Fees?.Select((x) => new WithdrawalFeeEntity
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
                DestinationTagType = withdrawal.DestinationDetails.TagType,
                UpdatedAt = withdrawal.UpdatedAt,
                CreatedAt = withdrawal.CreatedAt,
                DestinationAddress = withdrawal.DestinationDetails.Address,
                AccountId = withdrawal.AccountId,
                DestinationTag = withdrawal.DestinationDetails.Tag,
                UserContext = new UserContextEntity()
                {
                    AccountReferenceId = withdrawal.UserContext.AccountReferenceId,
                    ApiKeyId = withdrawal.UserContext.ApiKeyId,
                    PassClientIp = withdrawal.UserContext.PassClientIp,
                    UserId = withdrawal.UserContext.UserId,
                    WithdrawalReferenceId = withdrawal.UserContext.WithdrawalReferenceId
                }
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
                withdrawalEntity.Fees?
                    .Select(x => new Unit(x.AssetId, x.Amount))
                    .ToArray(),
                new DestinationDetails(
                    withdrawalEntity.DestinationAddress,
                    withdrawalEntity.DestinationTag,
                    withdrawalEntity.DestinationTagType),
                withdrawalEntity.State,
                withdrawalEntity.TransactionId != null ? new TransactionInfo(
                    withdrawalEntity.TransactionId,
                    // ReSharper disable once PossibleInvalidOperationException
                    withdrawalEntity.TransactionBlock.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    withdrawalEntity.TransactionRequiredConfirmationsCount.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    withdrawalEntity.TransactionDateTime.Value.UtcDateTime) : null,
                withdrawalEntity.WithdrawalErrorCode == null ?
                    null :
                    new WithdrawalError(withdrawalEntity.WithdrawalErrorMessage, withdrawalEntity.WithdrawalErrorCode.Value),
                withdrawalEntity.OperationId,
                withdrawalEntity.CreatedAt.UtcDateTime,
                withdrawalEntity.UpdatedAt.UtcDateTime,
                new UserContext()
                {
                    AccountReferenceId = withdrawalEntity.UserContext.AccountReferenceId,
                    UserId = withdrawalEntity.UserContext.UserId,
                    ApiKeyId = withdrawalEntity.UserContext.ApiKeyId,
                    WithdrawalReferenceId = withdrawalEntity.UserContext.WithdrawalReferenceId,
                    PassClientIp = withdrawalEntity.UserContext.PassClientIp
                });

            return withdrawal;
        }
    }
}
