using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Microsoft.EntityFrameworkCore;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    public class BrokerAccountDetailsRepository : IBrokerAccountDetailsRepository
    {
        private readonly DatabaseContext _dbContext;

        public BrokerAccountDetailsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<BrokerAccountDetails>> GetAnyOf(ISet<BrokerAccountDetailsId> ids)
        {
            var idStrings = ids.Select(x => x.ToString()).ToArray();

            var query = _dbContext
                .BrokerAccountsDetails
                .Where(x => idStrings.Contains(x.NaturalId));
            
            await query.LoadAsync();

            return query
                .AsEnumerable()
                .Select(MapToDomain)
                .ToArray();
        }

        public async Task<IReadOnlyDictionary<ActiveBrokerAccountDetailsId, BrokerAccountDetails>> GetActive(
            ISet<ActiveBrokerAccountDetailsId> ids)
        {
            var entities = new List<BrokerAccountDetailsEntity>();

            foreach (var id in ids)
            {
                var entity = await _dbContext
                    .BrokerAccountsDetails
                    .Where(x => x.ActiveId == id.ToString())
                    .OrderByDescending(x => x.Id)
                    .FirstAsync();

                entities.Add(entity);
            }

            return entities
                .Select(MapToDomain)
                .ToDictionary(
                    x => new ActiveBrokerAccountDetailsId(x.NaturalId.BlockchainId, x.BrokerAccountId), 
                    x => x);
        }

        public async Task<BrokerAccountDetails> GetActive(ActiveBrokerAccountDetailsId id)
        {
            var result = await _dbContext
                .BrokerAccountsDetails
                .Where(x => x.ActiveId == id.ToString())
                .OrderByDescending(x => x.Id)
                .FirstAsync();

            return MapToDomain(result);
        }

        public async Task Add(BrokerAccountDetails brokerAccount)
        {
            var entity = MapToEntity(brokerAccount);

            _dbContext.BrokerAccountsDetails.Add(entity);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<BrokerAccountDetails> Get(long id)
        {
            var details = await _dbContext
                .BrokerAccountsDetails
                .FirstAsync(x => x.Id == id);

            return MapToDomain(details);
        }

        public async Task<long> GetCountByBrokerAccountId(long brokerAccountId)
        {
            var count = await _dbContext.BrokerAccountsDetails.Where(x => x.BrokerAccountId == brokerAccountId).CountAsync();

            return count;
        }

        private static BrokerAccountDetailsEntity MapToEntity(BrokerAccountDetails details)
        {
            return new BrokerAccountDetailsEntity
            {
                BlockchainId = details.NaturalId.BlockchainId,
                TenantId = details.TenantId,
                BrokerAccountId = details.BrokerAccountId,
                Address = details.NaturalId.Address,
                Id = details.Id,
                NaturalId = details.NaturalId.ToString(),
                ActiveId = new ActiveBrokerAccountDetailsId(details.NaturalId.BlockchainId, details.BrokerAccountId).ToString(),
                CreatedAt = details.CreatedAt
            };
        }

        private static BrokerAccountDetails MapToDomain(BrokerAccountDetailsEntity entity)
        {
            if (entity == null)
                return null;

            var brokerAccount = BrokerAccountDetails.Restore(
                entity.Id,
                new BrokerAccountDetailsId(entity.BlockchainId, entity.Address),
                entity.TenantId,
                entity.BrokerAccountId,
                entity.CreatedAt.UtcDateTime
            );

            return brokerAccount;
        }
    }
}
