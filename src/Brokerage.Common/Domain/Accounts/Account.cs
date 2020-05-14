using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;

namespace Brokerage.Common.Domain.Accounts
{
    public class Account
    {
        private readonly List<object> _events;

        private Account(
            long id,
            long brokerAccountId,
            string referenceId,
            AccountState state,
            DateTime createdAt,
            DateTime updatedAt)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            ReferenceId = referenceId;
            State = state;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;

            _events = new List<object>();
        }

        public IReadOnlyCollection<object> Events => _events;
        public long Id { get; }
        public long BrokerAccountId { get; }
        public string ReferenceId { get; }
        public AccountState State { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }

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
                createdAt);

            account._events.Add(new AccountAdded()
            {
                BrokerAccountId = account.BrokerAccountId,
                CreatedAt = account.CreatedAt,
                AccountId = account.Id
            });

            return account;
        }

        public static Account Restore(
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState accountState,
            DateTime createdAt,
            DateTime updatedAt)
        {
            return new Account(
                accountId,
                brokerAccountId,
                referenceId,
                accountState,
                createdAt,
                updatedAt);
        }

        public async Task FinalizeCreation(
            ILogger<Account> logger,
            BrokerAccount brokerAccount,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            if (State == AccountState.Creating)
            {
                await RequestWalletGeneration(logger,
                    brokerAccount,
                    blockchainsRepository,
                    vaultAgentClient);
            }
        }

        private async Task RequestWalletGeneration(
            ILogger<Account> logger,
            BrokerAccount brokerAccount,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            string cursor = null;
            var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(new WalletGenerationRequesterContext()
            {
                AggregateId = this.Id,
                AggregateType = AggregateType.Account
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
                    // TODO: Decide if address or tag should be generated

                    var walletGenerationResponse = await vaultAgentClient.Wallets.GenerateAsync(
                        new GenerateWalletRequest
                        {
                            RequestId = $"Brokerage:AccountDetails:{Id}_{blockchain.Id}",
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

        private void Activate()
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
    }
}
