using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;

namespace Brokerage.Common.Domain.Accounts
{
    public class Account
    {
        private readonly List<object> _events;

        private Account(
            string requestId,
            long id,
            long brokerAccountId,
            string referenceId,
            AccountState state,
            DateTime createdAt,
            DateTime updatedAt)
        {
            RequestId = requestId;
            Id = id;
            BrokerAccountId = brokerAccountId;
            ReferenceId = referenceId;
            State = state;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;

            _events = new List<object>();
        }

        public IReadOnlyCollection<object> Events => _events;

        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public long Id { get; }
        public long BrokerAccountId { get; }
        public string ReferenceId { get; }
        public AccountState State { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        
        public static Account Create(
            string requestId,
            long brokerAccountId,
            string referenceId)
        {
            var createdAt = DateTime.UtcNow;
            return new Account(
                requestId,
                default,
                brokerAccountId,
                referenceId,
                AccountState.Creating,
                createdAt,
                createdAt);
        }

        public static Account Restore(
            string requestId,
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState accountState,
            DateTime createdAt,
            DateTime updatedAt)
        {
            return new Account(
                requestId,
                accountId,
                brokerAccountId,
                referenceId,
                accountState,
                createdAt,
                updatedAt);
        }

        public async Task FinalizeCreation(
            ILogger<Account> logger,
            IBlockchainsRepository blockchainsRepository, 
            IAccountDetailsRepository detailsRepository,
            IVaultAgentClient vaultAgentClient,
            IOutboxManager outboxManager)
        {
            if (State == AccountState.Creating)
            {
                await CreateDetails(logger,
                    blockchainsRepository,
                    detailsRepository,
                    vaultAgentClient,
                    outboxManager);

                Activate();
            }
            else
            {
                var accountDetails = await detailsRepository.GetByAccountAsync(Id);

                _events.Add(GetAccountDetailsAddedEvent(accountDetails));
                
                // ReSharper disable once PossibleInvalidOperationException
                _events.Add(GetAccountActivatedEvent(UpdatedAt));
            }
        }

        private async Task CreateDetails(
            ILogger<Account> logger,
            IBlockchainsRepository blockchainsRepository, 
            IAccountDetailsRepository detailsRepository,
            IVaultAgentClient vaultAgentClient,
            IOutboxManager outboxManager)
        {
            string cursor = null;

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
                    var outbox = await outboxManager.Open(
                        $"AccountDetails:Create:{Id}_{blockchain.Id}", 
                        () => detailsRepository.GetNextIdAsync());

                    if (!outbox.IsStored)
                    {
                        // TODO: Decide if address or tag should be generated

                        var walletGenerationResponse = await vaultAgentClient.Wallets.GenerateAsync(
                            new GenerateWalletRequest
                            {
                                RequestId = $"Brokerage:AccountDetails:{outbox.AggregateId}",
                                BlockchainId = blockchain.Id
                            });

                        if (walletGenerationResponse.BodyCase == GenerateWalletResponse.BodyOneofCase.Error)
                        {
                            logger.LogWarning("Wallet generation failed {@context}", walletGenerationResponse);

                            throw new InvalidOperationException($"Wallet generation has been failed: {walletGenerationResponse.Error.ErrorMessage}");
                        }

                        var accountDetails = AccountDetails.Create(
                            outbox.AggregateId,
                            new AccountDetailsId(blockchain.Id, walletGenerationResponse.Response.Address),
                            Id,
                            BrokerAccountId);
                        
                        // TODO: Batch
                        await detailsRepository.AddOrIgnoreAsync(accountDetails);

                        outbox.Publish(GetAccountDetailsAddedEvent(accountDetails));
                    }

                    foreach (var evt in outbox.Events)
                    {
                        _events.Add(evt);
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
