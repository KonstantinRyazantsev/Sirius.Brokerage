using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public class AccountRequisitesRepository : IAccountRequisitesRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public AccountRequisitesRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<IReadOnlyCollection<AccountRequisites>> GetByAccountAsync(
            long accountId,
            int limit,
            long? cursor,
            bool sortAsc)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var query = context
                .AccountRequisites
                .Where(x => x.AccountId == accountId);

            if (sortAsc)
            {
                if (cursor != null)
                {
                    query = query.Where(x => x.AccountId > cursor);
                }

                query = query.OrderBy(x => x.Id);
            }
            else
            {
                if (cursor != null)
                {
                    query = query.Where(x => x.AccountId < cursor);
                }

                query = query.OrderByDescending(x => x.Id);
            }

            query = query.Take(limit);

            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<AccountRequisites> AddOrGetAsync(AccountRequisites requisites)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(requisites);

            context.AccountRequisites.Add(newEntity);

            try
            {
                await context.SaveChangesAsync();

                return MapToDomain(newEntity);
            }
            catch (DbUpdateException e) //Check that request was already processed (by constraint)
                when (e.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == "23505" &&
                      pgEx.ConstraintName == "IX_BrokerAccountRequisites_RequestId")
            {
                var entity = await context
                    .AccountRequisites
                    .FirstAsync(x => x.RequestId == requisites.RequestId);

                return MapToDomain(entity);
            }
        }

        public async Task UpdateAsync(AccountRequisites requisites)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            
            var entity = MapToEntity(requisites);
            
            context.AccountRequisites.Update(entity);

            await context.SaveChangesAsync();
        }

        private static AccountRequisitesEntity MapToEntity(AccountRequisites requisites)
        {
            var tagType = requisites.TagType.HasValue ? (TagTypeEnum?)(requisites.TagType.Value switch
            {
                DestinationTagType.Number => TagTypeEnum.Number,
                DestinationTagType.Text => TagTypeEnum.Text,
                _ => throw  new ArgumentOutOfRangeException(nameof(requisites.TagType), requisites.TagType.Value, null)

            }) : null;

            return new AccountRequisitesEntity
            {
                RequestId = requisites.RequestId,
                Address = requisites.Address,
                Id = requisites.AccountRequisitesId,
                AccountId = requisites.AccountId,
                BlockchainId = requisites.BlockchainId,
                Tag = requisites.Tag,
                TagType = tagType,
                CreationDateTime = requisites.CreationDateTime
            };
        }

        private static AccountRequisites MapToDomain(AccountRequisitesEntity entity)
        {
            var tagType = entity.TagType.HasValue ? (DestinationTagType?)(entity.TagType.Value switch
            {
                TagTypeEnum.Number => DestinationTagType.Number,
                TagTypeEnum.Text => DestinationTagType.Text,
                _ => throw new ArgumentOutOfRangeException(nameof(entity.TagType), entity.TagType.Value, null)

            }) : null;

            var brokerAccount = AccountRequisites.Restore(
                entity.RequestId,
                entity.Id,
                entity.AccountId,
                entity.BlockchainId,
                entity.Address,
                entity.Tag,
                tagType,
                entity.CreationDateTime.UtcDateTime);

            return brokerAccount;
        }
    }
}
