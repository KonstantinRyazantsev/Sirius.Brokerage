using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;
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
        public long Sequence { get; }

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

            account._events.Add(new AccountUpdated()
            {
                BrokerAccountId = account.BrokerAccountId,
                CreatedAt = account.CreatedAt,
                AccountId = account.Id,
                Sequence = account.Sequence,
                UpdatedAt = account.UpdatedAt,
                State = account.State,
                ReferenceId = account.ReferenceId
            });

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
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory)
        {
            if (State == AccountState.Creating)
            {
                await RequestDetailsGeneration(
                    logger,
                    brokerAccount,
                    blockchainsRepository,
                    vaultAgentClient,
                    destinationTagGeneratorFactory
                );
            }
        }

        private async Task RequestDetailsGeneration(
            ILogger<Account> logger,
            BrokerAccount brokerAccount,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory)
        {
            string cursor = null;
            var expectedCount = await blockchainsRepository.GetCountAsync();
            var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(new WalletGenerationRequesterContext
            {
                AggregateId = Id,
                AggregateType = AggregateType.Account,
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
                    var tagGenerator = destinationTagGeneratorFactory.CreateOrDefault(blockchain);

                    if (tagGenerator != null)
                    {
                        _commands.Add(new CreateAccountDetailsForTag
                        {
                            AccountId = Id,
                            BlockchainId = blockchain.Id,
                            ExpectedCount = expectedCount
                        });

                        continue;
                    }

                    var walletGenerationResponse = await vaultAgentClient.Wallets.GenerateAsync(
                        new GenerateWalletRequest
                        {
                            RequestId = $"Brokerage:AccountDetails:{Id}:{blockchain.Id}",
                            BlockchainId = blockchain.Id,
                            TenantId = brokerAccount.TenantId,
                            VaultId = brokerAccount.VaultId,
                            Component = nameof(Brokerage),
                            Context = requesterContext
                        });

                    if (walletGenerationResponse.BodyCase == GenerateWalletResponse.BodyOneofCase.Error)
                    {
                        logger.LogWarning("Wallet generation failed {@context}", walletGenerationResponse);

                        throw new InvalidOperationException($"Wallet generation has been failed: {walletGenerationResponse.Error.ErrorMessage}");
                    }
                }
            } while (true);
        }

        public void Activate()
        {
            State = AccountState.Active;
            UpdatedAt = DateTime.UtcNow;

            _events.Add(GetAccountActivatedEvent(UpdatedAt));
        }

        private AccountActivated GetAccountActivatedEvent(DateTime activationDateTime)
        {
            return new AccountActivated
            {
                UpdatedAt = activationDateTime,
                AccountId = Id
            };
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

        public async Task AddAccountDetails(
            IAccountDetailsRepository accountDetailsRepository,
            IAccountsRepository accountsRepository,
            AccountDetails accountDetails,
            long expectedCount)
        {
            await accountDetailsRepository.Add(accountDetails);
            
            _events.Add(GetAccountDetailsAddedEvent(accountDetails));

            var accountDetailsCount = await accountDetailsRepository.GetCountByAccountId(Id);

            if (accountDetailsCount >= expectedCount)
            {
                Activate();
                await accountsRepository.Update(this);
            }
        }
    }
}
