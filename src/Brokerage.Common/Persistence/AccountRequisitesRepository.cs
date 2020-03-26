using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.AccountRequisites;
using Brokerage.Common.Domain.BrokerAccountRequisites;
using Brokerage.Common.Persistence.DbContexts;
using Brokerage.Common.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
                .Include(x => x.Account)
                .Include(x => x.Account.BrokerAccount)
                .FirstOrDefaultAsync(x => x.Id == id);

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
                    .FirstOrDefaultAsync(x => x.RequestId == brokerAccount.RequestId);

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
                TagType.Number => TagTypeEnum.Number,
                TagType.Text => TagTypeEnum.Text,
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

            var tagType = entity.TagType.HasValue ? (TagType?)(entity.TagType.Value switch
            {
                TagTypeEnum.Number => TagType.Number,
                TagTypeEnum.Text => TagType.Text,
                _ => throw new ArgumentOutOfRangeException(nameof(entity.TagType), entity.TagType.Value, null)

            }) : null;

            var brokerAccount = AccountRequisites.Restore(
                entity.RequestId,
                entity.Id,
                entity.AccountId,
                entity.Account.BrokerAccount.TenantId,
                entity.BlockchainId,
                entity.Address,
                entity.Tag,
                tagType);

            return brokerAccount;
        }
    }
}
