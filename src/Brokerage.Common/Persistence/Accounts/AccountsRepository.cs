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

        public async Task<Account> AddOrGetAsync(Account brokerAccount)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(brokerAccount);

            context.Accounts.Add(newEntity);

            try
            {
                await context.SaveChangesAsync();

                return MapToDomain(newEntity);
            }
            catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx &&
                                              pgEx.SqlState == "23505" &&
                                              pgEx.ConstraintName == "IX_Account_RequestId")
            {
                var entity = await context
                    .Accounts
                    .FirstAsync(x => x.RequestId == brokerAccount.RequestId);

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
                RequestId = domainModel.RequestId,
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
                entity.RequestId,
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
