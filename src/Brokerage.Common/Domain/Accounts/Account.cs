using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccounts;
using Brokerage.Common.ReadModels.Blockchains;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;

namespace Brokerage.Common.Domain.Accounts
{
    public class Account
    {
        private readonly List<object> _events;
        private readonly List<object> _commands;

        private Account(
            long id,
            long brokerAccountId,
            string referenceId,
            AccountState state,
            DateTime createdAt,
            DateTime updatedAt,
            long sequence)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            ReferenceId = referenceId;
            State = state;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Sequence = sequence;

            _events = new List<object>();
            _commands = new List<object>();
        }

        public IReadOnlyCollection<object> Events => _events;
        public IReadOnlyCollection<object> Commands => _commands;

        public long Id { get; }
        public long BrokerAccountId { get; }
        public string ReferenceId { get; }
        public AccountState State { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public long Sequence { get; private set; }

        public static Account Create(
            long id,
            long brokerAccountId,
            string referenceId)
        {
            var createdAt = DateTime.UtcNow;
            var account = new Account(
                id,
                brokerAccountId,
                referenceId,
                AccountState.Creating,
                createdAt,
                createdAt,
                0);

            account.AddAccountUpdatedEvent();

            return account;
        }

        public static Account Restore(
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState accountState,
            DateTime createdAt,
            DateTime updatedAt,
            long sequence)
        {
            return new Account(
                accountId,
                brokerAccountId,
                referenceId,
                accountState,
                createdAt,
                updatedAt,
                sequence);
        }

        public async Task FinalizeCreation(
            ILogger<Account> logger,
            BrokerAccount brokerAccount,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            IAccountsRepository accountsRepository)
        {
            if (State == AccountState.Creating)
            {
                var blockchains = await blockchainsRepository.GetByIds(brokerAccount.BlockchainIds);

                if (!blockchains.Any())
                {
                    Activate();
                    await accountsRepository.Update(this);

                    return;
                }

                var walletGenerationRequesterContext = new WalletGenerationRequesterContext
                {
                    RootId = brokerAccount.Id,
                    AggregateId = Id,
                    WalletGenerationReason = WalletGenerationReason.Account,
                    ExpectedBlockchainsCount = brokerAccount.BlockchainIds.Count
                };

                await RequestDetailsGeneration(
                    logger,
                    brokerAccount,
                    vaultAgentClient,
                    destinationTagGeneratorFactory,
                    blockchains,
                    walletGenerationRequesterContext
                );
            }
        }

        public async Task AddBlockchains(
            ILogger<Account> logger,
            BrokerAccount brokerAccount,
            IVaultAgentClient vaultAgentClient,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            IReadOnlyCollection<Blockchain> blockchains,
            int expectedAccountsCount)
        {
            var walletGenerationContext = new WalletGenerationRequesterContext()
            {
                RootId = brokerAccount.Id,
                AggregateId = this.Id,
                WalletGenerationReason = WalletGenerationReason.AccountUpdate,
                ExpectedBlockchainsCount = brokerAccount.BlockchainIds.Count,
                ExpectedAccountsCount = expectedAccountsCount
            };

            await RequestDetailsGeneration(
                logger,
                brokerAccount,
                vaultAgentClient,
                destinationTagGeneratorFactory,
                blockchains,
                walletGenerationContext);
        }

        public async Task AddAccountDetails(
            IAccountDetailsRepository accountDetailsRepository,
            IAccountsRepository accountsRepository,
            AccountDetails accountDetails,
            long expectedBlockchainsCount)
        {
            await accountDetailsRepository.Add(accountDetails);

            _events.Add(GetAccountDetailsAddedEvent(accountDetails));

            var accountDetailsCount = await accountDetailsRepository.GetCountByAccountId(Id);

            if (accountDetailsCount >= expectedBlockchainsCount)
            {
                if (Activate())
                {
                    await accountsRepository.Update(this);
                }
            }
        }

        public async Task AddAccountDetails(
            IBrokerAccountsRepository brokerAccountsRepository,
            IAccountDetailsRepository accountDetailsRepository,
            IAccountsRepository accountsRepository,
            AccountDetails accountDetails,
            BrokerAccount brokerAccount,
            long expectedBlockchainsCount,
            long expectedAccountsCount)
        {
            await accountDetailsRepository.Add(accountDetails);

            _events.Add(GetAccountDetailsAddedEvent(accountDetails));

            var accountDetailsCount = await accountDetailsRepository.GetCountByAccountId(Id);

            if (accountDetailsCount >= expectedBlockchainsCount)
            {
                if (Activate())
                {
                    await accountsRepository.Update(this);
                }
            }

            var accountDetailsCountForBrokerAccount = await accountDetailsRepository.GetCountForBrokerAccountId(brokerAccount.Id);
            var expectedDetailsCount = expectedAccountsCount * expectedBlockchainsCount;
            if (brokerAccount.State == BrokerAccountState.Updating && accountDetailsCountForBrokerAccount >= expectedDetailsCount)
            {
                brokerAccount.Activate();
                await brokerAccountsRepository.Update(brokerAccount);
            }
        }

        private bool Activate()
        {
            if (State == AccountState.Creating ||
                State == AccountState.Blocked)
            {
                State = AccountState.Active;
                UpdatedAt = DateTime.UtcNow;
                Sequence++;

                AddAccountUpdatedEvent();
                return true;
            }

            return false;
        }

        private async Task RequestDetailsGeneration(
            ILogger<Account> logger,
            BrokerAccount brokerAccount,
            IVaultAgentClient vaultAgentClient,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            IReadOnlyCollection<Blockchain> blockchains,
            WalletGenerationRequesterContext walletGenerationRequesterContext)
        {
            var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(walletGenerationRequesterContext);

            foreach (var blockchainId in blockchains)
            {
                var tagGenerator = destinationTagGeneratorFactory.CreateOrDefault(blockchainId);

                if (tagGenerator != null)
                {
                    _commands.Add(new CreateAccountDetailsForTag
                    {
                        AccountId = Id,
                        BlockchainId = blockchainId.Id,
                        ExpectedBlockchainsCount = walletGenerationRequesterContext.ExpectedBlockchainsCount,
                        ExpectedAccountsCount = walletGenerationRequesterContext.ExpectedAccountsCount
                    });

                    continue;
                }

                var walletGenerationRequest = new GenerateWalletRequest
                {
                    RequestId = $"Brokerage:AccountDetails:{Id}:{blockchainId.Id}",
                    BlockchainId = blockchainId.Id,
                    TenantId = brokerAccount.TenantId,
                    VaultId = brokerAccount.VaultId,
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

                    throw new InvalidOperationException($"Wallet generation has been failed: {walletGenerationResponse.Error.ErrorMessage}");
                }
            }
        }

        private static AccountDetailsAdded GetAccountDetailsAddedEvent(AccountDetails details)
        {
            return new AccountDetailsAdded
            {
                CreatedAt = details.CreatedAt,
                Address = details.NaturalId.Address,
                BlockchainId = details.NaturalId.BlockchainId,
                Tag = details.NaturalId.Tag,
                TagType = details.NaturalId.TagType,
                AccountId = details.AccountId,
                AccountDetailsId = details.Id
            };
        }

        private void AddAccountUpdatedEvent()
        {
            this._events.Add(
                new AccountUpdated()
                {
                    BrokerAccountId = BrokerAccountId,
                    CreatedAt = CreatedAt,
                    AccountId = Id,
                    Sequence = Sequence,
                    UpdatedAt = UpdatedAt,
                    State = State,
                    ReferenceId = ReferenceId
                });
        }
    }
}
