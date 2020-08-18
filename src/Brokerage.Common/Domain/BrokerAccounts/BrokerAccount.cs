using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccounts;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;

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
            long vaultId,
            long sequence)
        {
            Id = id;
            Name = name;
            TenantId = tenantId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            State = state;
            VaultId = vaultId;
            Sequence = sequence;
        }
        
        public long Id { get; }
        public string Name { get; }
        public string TenantId { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public BrokerAccountState State { get; private set; }
        public long VaultId { get; }
        public long Sequence { get; }
        public IReadOnlyCollection<object> Events => _events;

        public static BrokerAccount Create(long id, string name, string tenantId, long vaultId)
        {
            var utcNow = DateTime.UtcNow;
            var brokerAccount = new BrokerAccount(
                id,
                name, 
                tenantId,
                utcNow, 
                utcNow, 
                BrokerAccountState.Creating, 
                vaultId,
                0);

            brokerAccount.AddBrokerAccountUpdatedEvent();

            return brokerAccount;
        }

        public static BrokerAccount Restore(
            long id,
            string name,
            string tenantId,
            DateTime createdAt,
            DateTime updatedAt,
            BrokerAccountState state,
            long vaultId,
            long sequence)
        {
            return new BrokerAccount(
                id, 
                name, 
                tenantId, 
                createdAt, 
                updatedAt, 
                state, 
                vaultId,
                sequence);
        }

        public void Activate()
        {
            UpdatedAt = DateTime.UtcNow;
            State = BrokerAccountState.Active;
            
            _events.Add(new BrokerAccountActivated
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
            
            _events.Add(new BrokerAccountDetailsAdded
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

        public async Task FinalizeCreation(ILogger<BrokerAccount> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            if (State == BrokerAccountState.Creating)
            {
                await RequestDetailsGeneration(logger, blockchainsRepository, vaultAgentClient);
            }
        }
        
        private async Task RequestDetailsGeneration(ILogger<BrokerAccount> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            string cursor = null;
            var expectedCount = await blockchainsRepository.GetCountAsync();
            var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(new WalletGenerationRequesterContext
            {
                AggregateId = Id,
                AggregateType = AggregateType.BrokerAccount,
                ExpectedCount = expectedCount
            });

            do
            {
                var blockchains = await blockchainsRepository.GetAllAsync(cursor, 100);

                if (!blockchains.Any())
                {
                    break;
                }

                cursor = blockchains.Last().Id;

                foreach (var blockchain in blockchains)
                {
                    var walletGenerationRequest = new GenerateWalletRequest
                    {
                        RequestId = $"Brokerage:BrokerAccountDetails:{Id}:{blockchain.Id}",
                        BlockchainId = blockchain.Id,
                        TenantId = TenantId,
                        VaultId = VaultId,
                        Component = nameof(Brokerage),
                        Context = requesterContext
                    };

                    var walletGenerationResponse = await vaultAgentClient.Wallets.GenerateAsync(walletGenerationRequest);

                    if (walletGenerationResponse.BodyCase == GenerateWalletResponse.BodyOneofCase.Error)
                    {
                        logger.LogWarning("Wallet generation failed {@context}", new
                        {
                            Request = walletGenerationRequest,
                            Response = walletGenerationResponse
                        });

                        throw new InvalidOperationException($"Wallet generation request has been failed: {walletGenerationResponse.Error.ErrorMessage}");
                    }
                }
            } while (true);
        }

        private void AddBrokerAccountUpdatedEvent()
        {
            _events.Add(new BrokerAccountUpdated
            {
                BrokerAccountId = Id,
                UpdatedAt = UpdatedAt,
                VaultId = VaultId,
                Sequence = Sequence,
                CreatedAt = CreatedAt,
                Name = Name,
                TenantId = TenantId,
                State = State
            });
        }
    }
}
