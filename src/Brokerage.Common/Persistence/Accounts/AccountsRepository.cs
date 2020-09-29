using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;

namespace Brokerage.Common.Persistence.Accounts
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly DatabaseContext _dbContext;

        public AccountsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Account> Get(long accountId)
        {
            var entity = await _dbContext
                .Accounts
                .FirstAsync(x => x.Id == accountId);

            return MapToDomain(entity);
        }

        public async Task<Account> GetOrDefault(long accountId)
        {
            var entity = await _dbContext
                .Accounts
                .FirstOrDefaultAsync(x => x.Id == accountId);

            return MapToDomain(entity);
        }

        public async Task Update(Account account)
        {
            var entity = MapToEntity(account);
            
            _dbContext.Accounts.Update(entity);
            
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<Account>> GetForBrokerAccount(long brokerAccountId, long cursor, int limit)
        {
            var accounts = _dbContext.Accounts
                .Where(x => x.BrokerAccountId == brokerAccountId &&
                            x.Id > cursor)
                .Take(limit);

            await accounts.LoadAsync();

            return accounts
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<int> GetCountForBrokerAccountId(long brokerAccountId, AccountState? accountState)
        {
            var state = accountState.HasValue ? accountState.Value switch
            {
                AccountState.Creating => AccountStateEnum.Creating,
                AccountState.Active => AccountStateEnum.Active,
                AccountState.Blocked => AccountStateEnum.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(accountState), accountState, null)
            } : (AccountStateEnum?)null;

            var query = _dbContext.Accounts
                .Where(x => x.BrokerAccountId == brokerAccountId);

            if (state != null)
            {
                query = query.Where(x => x.State == state);
            }

            var count = await query.CountAsync();

            return count;
        }

        public async Task<IReadOnlyCollection<Account>> GetAnyOf(long[] accountIds)
        {
            var accounts = _dbContext.Accounts
                .Where(x => accountIds.Contains(x.Id));

            await accounts.LoadAsync();

            return accounts
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task Add(Account account)
        {
            var newEntity = MapToEntity(account);

            _dbContext.Accounts.Add(newEntity);

            await _dbContext.SaveChangesAsync();
        }

        private static Account MapToDomain(AccountEntity entity)
        {
            if (entity == null)
                return null;

            var state = entity.State switch
            {
                AccountStateEnum.Active => AccountState.Active,
                AccountStateEnum.Blocked => AccountState.Blocked,
                AccountStateEnum.Creating => AccountState.Creating,
                _ => throw new ArgumentOutOfRangeException(nameof(entity.State), entity.State, null)
            };

            var brokerAccount = Account.Restore(
                entity.Id,
                entity.BrokerAccountId,
                entity.ReferenceId,
                state,
                entity.CreatedAt.UtcDateTime,
                entity.UpdatedAt.UtcDateTime,
                entity.Sequence
            );

            return brokerAccount;
        }

        private static AccountEntity MapToEntity(Account domainModel)
        {
            var state = domainModel.State switch
            {
                AccountState.Active =>   AccountStateEnum.Active,
                AccountState.Blocked =>  AccountStateEnum.Blocked,
                AccountState.Creating => AccountStateEnum.Creating,

                _ => throw new ArgumentOutOfRangeException(nameof(domainModel.State), domainModel.State, null)
            };

            var entity = new AccountEntity
            {
                State = state,
                CreatedAt = domainModel.CreatedAt,
                UpdatedAt = domainModel.UpdatedAt,
                BrokerAccountId = domainModel.BrokerAccountId,
                ReferenceId = domainModel.ReferenceId,
                Id = domainModel.Id,
                Sequence = domainModel.Sequence
            };

            return entity;
        }
    }
}
