using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Persistence.BrokerAccounts;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccount
    {
        private readonly List<object> _events = new List<object>();

        private BrokerAccount(
            long id, 
            string name,
            string tenantId, 
            DateTime createdAt, 
            DateTime updatedAt, 
            BrokerAccountState state,
            long vaultId)
        {
            Id = id;
            Name = name;
            TenantId = tenantId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            State = state;
            VaultId = vaultId;
        }
        
        public long Id { get; }
        public string Name { get; }
        public string TenantId { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public BrokerAccountState State { get; private set; }
        public long VaultId { get; }

        public IReadOnlyCollection<object> Events => _events;

        public static BrokerAccount Create(long id, string name, string tenantId, long vaultId)
        {
            var utcNow = DateTime.UtcNow;

            return new BrokerAccount(
                id,
                name, 
                tenantId,
                utcNow, 
                utcNow, 
                BrokerAccountState.Creating, 
                vaultId);
        }

        public static BrokerAccount Restore(
            long id,
            string name,
            string tenantId,
            DateTime createdAt,
            DateTime updatedAt,
            BrokerAccountState state,
            long vaultId)
        {
            return new BrokerAccount(
                id, 
                name, 
                tenantId, 
                createdAt, 
                updatedAt, 
                state, 
                vaultId);
        }

        public void Activate()
        {
            UpdatedAt = DateTime.UtcNow;
            State = BrokerAccountState.Active;
            
            _events.Add(new BrokerAccountActivated()
            {
                BrokerAccountId = Id,
                UpdatedAt = UpdatedAt
            });
        }

        public async Task AddBrokerAccountDetails(
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IBrokerAccountsRepository brokerAccountsRepository,
            BrokerAccountDetails brokerAccountDetails,
            long expectedCount)
        {
            await brokerAccountDetailsRepository.Add(brokerAccountDetails);
            
            _events.Add(new BrokerAccountDetailsAdded()
            {
                BlockchainId = brokerAccountDetails.NaturalId.BlockchainId,
                Address = brokerAccountDetails.NaturalId.Address,
                BrokerAccountId = brokerAccountDetails.BrokerAccountId,
                BrokerAccountDetailsId = brokerAccountDetails.Id,
                CreatedAt = brokerAccountDetails.CreatedAt
            });

            var accountDetailsCount = await brokerAccountDetailsRepository.GetCountByBrokerAccountId(Id);

            if (accountDetailsCount >= expectedCount)
            {
                Activate();
                await brokerAccountsRepository.Update(this);
            }
        }
    }
}
