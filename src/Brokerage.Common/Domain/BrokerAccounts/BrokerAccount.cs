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
        private readonly List<object> _commands = new List<object>();
        private readonly HashSet<string> _blockchainIds = new HashSet<string>();

        private BrokerAccount(
            long id,
            string name,
            string tenantId,
            DateTime createdAt,
            DateTime updatedAt,
            BrokerAccountState state,
            long vaultId,
            long sequence,
            IReadOnlyCollection<string> blockchainIds)
        {
            Id = id;
            Name = name;
            TenantId = tenantId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            State = state;
            VaultId = vaultId;
            Sequence = sequence;

            if (blockchainIds != null && blockchainIds.Any())
            {
                _blockchainIds.AddRange(blockchainIds);
            }
        }

        public long Id { get; }
        public string Name { get; }
        public string TenantId { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public BrokerAccountState State { get; private set; }
        public long VaultId { get; }
        public long Sequence { get; private set; }
        public IReadOnlyCollection<string> BlockchainIds => _blockchainIds;
        public IReadOnlyCollection<object> Events => _events;

        public IReadOnlyCollection<object> Commands => _commands;

        public static BrokerAccount Create(long id, string name, string tenantId, long vaultId, IReadOnlyCollection<string> blockchainIds)
        {
            var utcNow = DateTime.UtcNow;

            var state = blockchainIds == null || !blockchainIds.Any() ? BrokerAccountState.Active : BrokerAccountState.Creating;

            var brokerAccount = new BrokerAccount(
                id,
                name,
                tenantId,
                utcNow,
                utcNow,
                state,
                vaultId,
                0,
                blockchainIds);

            brokerAccount.AddBrokerAccountUpdatedEvent();
            brokerAccount._commands.Add(new FinalizeBrokerAccountCreation
            {
                BrokerAccountId = brokerAccount.Id
            });

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
            long sequence,
            IReadOnlyCollection<string> blockchainIds)
        {
            return new BrokerAccount(
                id,
                name,
                tenantId,
                createdAt,
                updatedAt,
                state,
                vaultId,
                sequence,
                blockchainIds);
        }

        public void AddBlockchain(IReadOnlyCollection<string> blockchainIds)
        {
            SwitchState(new BrokerAccountState[]
            {
                BrokerAccountState.Active
            }, BrokerAccountState.Updating);

            foreach (var blockchainId in blockchainIds)
            {
                _blockchainIds.Add(blockchainId);
            }

            _commands.Add(new AddBlockchainToBrokerAccount()
            {
                BlockchainIds = blockchainIds,
                BrokerAccountId = this.Id
            });

            AddBrokerAccountUpdatedEvent();
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
                if (State != BrokerAccountState.Updating)
                {
                    Activate();
                    await brokerAccountsRepository.Update(this);
                }
            }
        }

        public async Task FinalizeCreation(ILogger<BrokerAccount> logger,
            IVaultAgentClient vaultAgentClient)
        {
            if (State == BrokerAccountState.Creating)
            {
                await RequestDetailsGeneration(
                    logger,
                    vaultAgentClient,
                    this.BlockchainIds.Count,
                    this.BlockchainIds);
            }
        }

        public async Task FinalizeBlockchainAdd(ILogger<BrokerAccount> logger,
            IVaultAgentClient vaultAgentClient,
            IReadOnlyCollection<string> blockchainIds)
        {
            if (State == BrokerAccountState.Updating)
            {
                await RequestDetailsGeneration(
                    logger,
                    vaultAgentClient,
                    this.BlockchainIds.Count,
                    blockchainIds);
            }
        }

        private void Activate()
        {
            SwitchState(new BrokerAccountState[]
                {
                    BrokerAccountState.Blocked,
                    BrokerAccountState.Updating,
                    BrokerAccountState.Creating
                },
                BrokerAccountState.Active);

            _events.Add(new BrokerAccountActivated
            {
                BrokerAccountId = Id,
                UpdatedAt = UpdatedAt
            });
        }

        private async Task RequestDetailsGeneration(ILogger<BrokerAccount> logger,
            IVaultAgentClient vaultAgentClient,
            int expectedCount,
            IReadOnlyCollection<string> blockchainIds)
        {
            if (!blockchainIds.Any())
            {
                return;
            }

            var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(new WalletGenerationRequesterContext
            {
                AggregateId = Id,
                AggregateType = AggregateType.BrokerAccount,
                ExpectedCount = expectedCount
            });

            foreach (var blockchainId in blockchainIds)
            {
                var walletGenerationRequest = new GenerateWalletRequest
                {
                    RequestId = $"Brokerage:BrokerAccountDetails:{Id}:{blockchainId}",
                    BlockchainId = blockchainId,
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
        }

        private void SwitchState(IEnumerable<BrokerAccountState> allowedStates, BrokerAccountState targetState)
        {
            if (!allowedStates.Contains(State))
            {
                throw new InvalidOperationException($"Can't switch broker account to the {targetState} from the state {State}");
            }

            UpdatedAt = DateTime.UtcNow;
            Sequence++;

            State = targetState;
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
                State = State,
                BlockchainIds = this.BlockchainIds
            });
        }
    }
}
