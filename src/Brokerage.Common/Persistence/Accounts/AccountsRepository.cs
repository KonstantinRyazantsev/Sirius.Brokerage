﻿using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Brokerage.Common.Persistence.Accounts
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public AccountsRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<long> GetNextIdAsync()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            return await context.GetNextId(Tables.Accounts, nameof(AccountEntity.Id));
        }

        public async Task<Account> GetAsync(long accountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .Accounts
                .FirstAsync(x => x.Id == accountId);

            return MapToDomain(entity);
        }

        public async Task<Account> GetOrDefaultAsync(long accountId)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .Accounts
                .FirstOrDefaultAsync(x => x.Id == accountId);

            return MapToDomain(entity);
        }

        public async Task UpdateAsync(Account account)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(account);
            
            context.Accounts.Update(entity);
            
            await context.SaveChangesAsync();
        }

        public async Task<Account> AddOrGetAsync(Account account)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(account);

            context.Accounts.Add(newEntity);

            try
            {
                await context.SaveChangesAsync();

                return MapToDomain(newEntity);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx &&
                                              pgEx.SqlState == "23505")
            {
                var entity = await context
                    .Accounts
                    .FirstAsync(x => x.Id == account.Id);

                return MapToDomain(entity);
            }
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
            };

            return entity;
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
                entity.UpdatedAt.UtcDateTime
            );

            return brokerAccount;
        }
    }
}
