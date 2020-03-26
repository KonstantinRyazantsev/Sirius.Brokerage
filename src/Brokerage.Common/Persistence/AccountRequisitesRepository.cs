﻿using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.AccountRequisites;
using Brokerage.Common.Domain.BrokerAccountRequisites;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public class AccountRequisitesRepository : IAccountRequisitesRepository
    {
        private readonly DbContextOptionsBuilder<BrokerageContext> _dbContextOptionsBuilder;

        public AccountRequisitesRepository(DbContextOptionsBuilder<BrokerageContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }


        public async Task<AccountRequisites> GetAsync(string brokerAccountRequisitesId)
        {
            long.TryParse(brokerAccountRequisitesId, out var id);
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var entity = await context
                .AccountRequisites
                .FirstAsync(x => x.Id == id);

            return MapToDomain(entity);
        }

        public async Task<AccountRequisites> AddOrGetAsync(AccountRequisites brokerAccount)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);

            var newEntity = MapToEntity(brokerAccount);

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
                    .FirstAsync(x => x.RequestId == brokerAccount.RequestId);

                return MapToDomain(entity);
            }
        }

        public async Task UpdateAsync(AccountRequisites brokerAccount)
        {
            await using var context = new BrokerageContext(_dbContextOptionsBuilder.Options);
            var entity = MapToEntity(brokerAccount);
            context
                .AccountRequisites
                .Update(entity);
            await context.SaveChangesAsync();
        }

        private AccountRequisitesEntity MapToEntity(AccountRequisites domainModel)
        {
            var tagType = domainModel.TagType.HasValue ? (TagTypeEnum?)(domainModel.TagType.Value switch
            {
                DestinationTagType.Number => TagTypeEnum.Number,
                DestinationTagType.Text => TagTypeEnum.Text,
                _ => throw  new ArgumentOutOfRangeException(nameof(domainModel.TagType), domainModel.TagType.Value, null)

            }) : null;

            return new AccountRequisitesEntity()
            {
                RequestId = domainModel.RequestId,
                Address = domainModel.Address,
                Id = domainModel.AccountRequisitesId,
                AccountId = domainModel.AccountId,
                BlockchainId = domainModel.BlockchainId,
                Tag = domainModel.Tag,
                TagType = tagType
            };
        }

        private AccountRequisites MapToDomain(AccountRequisitesEntity entity)
        {
            if (entity == null)
                return null;

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
                tagType);

            return brokerAccount;
        }
    }
}