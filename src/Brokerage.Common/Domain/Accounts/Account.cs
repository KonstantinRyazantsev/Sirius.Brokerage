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
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState state,
            DateTime creationDateTime,
            DateTime? blockingDateTime,
            DateTime? activationDateTime)
        {
            RequestId = requestId;
            AccountId = accountId;
            BrokerAccountId = brokerAccountId;
            ReferenceId = referenceId;
            State = state;
            CreationDateTime = creationDateTime;
            BlockingDateTime = blockingDateTime;
            ActivationDateTime = activationDateTime;

            _events = new List<object>();
        }

        public IReadOnlyCollection<object> Events => _events;

        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public long AccountId { get; }
        public long BrokerAccountId { get; }
        public string ReferenceId { get; }
        public AccountState State { get; private set; }
        public DateTime CreationDateTime { get; }
        public DateTime? BlockingDateTime { get; }
        public DateTime? ActivationDateTime { get; private set; }
        
        public static Account Create(
            string requestId,
            long brokerAccountId,
            string referenceId)
        {
            return new Account(
                requestId,
                default,
                brokerAccountId,
                referenceId,
                AccountState.Creating,
                DateTime.UtcNow,
                null,
                null);
        }

        public static Account Restore(
            string requestId,
            long accountId,
            long brokerAccountId,
            string referenceId,
            AccountState accountState,
            DateTime creationDateTime,
            DateTime? blockingDateTime,
            DateTime? activationDateTime)
        {
            return new Account(
                requestId,
                accountId,
                brokerAccountId,
                referenceId,
                accountState,
                creationDateTime,
                blockingDateTime,
                activationDateTime);
        }

        public async Task FinalizeCreation(
            ILogger<Account> logger,
            IBlockchainsRepository blockchainsRepository, 
            IAccountRequisitesRepository requisitesRepository,
            IVaultAgentClient vaultAgentClient,
            IOutboxManager outboxManager)
        {
            if (State == AccountState.Creating)
            {
                await CreateRequisites(logger,
                    blockchainsRepository,
                    requisitesRepository,
                    vaultAgentClient,
                    outboxManager);

                Activate();
            }
            else
            {
                var requisites = await requisitesRepository.GetByAccountAsync(AccountId);

                _events.Add(GetAccountRequisitesAddedEvent(requisites));
                
                // ReSharper disable once PossibleInvalidOperationException
                _events.Add(GetAccountActivatedEvent(ActivationDateTime.Value));
            }
        }

        private async Task CreateRequisites(
            ILogger<Account> logger,
            IBlockchainsRepository blockchainsRepository, 
            IAccountRequisitesRepository requisitesRepository,
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
                        $"AccountRequisites:Create:{AccountId}_{blockchain.Id}", 
                        () => requisitesRepository.GetNextIdAsync());

                    if (!outbox.IsStored)
                    {
                        // TODO: Decide if address or tag should be generated

                        var walletGenerationResponse = await vaultAgentClient.Wallets.GenerateAsync(
                            new GenerateWalletRequest
                            {
                                RequestId = $"Brokerage:AccountRequisites:{outbox.AggregateId}",
                                BlockchainId = blockchain.Id
                            });

                        if (walletGenerationResponse.BodyCase == GenerateWalletResponse.BodyOneofCase.Error)
                        {
                            logger.LogWarning("Wallet generation failed {@context}", walletGenerationResponse);

                            throw new InvalidOperationException($"Wallet generation has been failed: {walletGenerationResponse.Error.ErrorMessage}");
                        }

                        var requisites = AccountRequisites.Create(
                            outbox.AggregateId,
                            new AccountRequisitesId(blockchain.Id, walletGenerationResponse.Response.Address),
                            AccountId,
                            BrokerAccountId);
                        
                        // TODO: Batch
                        await requisitesRepository.AddOrIgnoreAsync(requisites);

                        outbox.Publish(GetAccountRequisitesAddedEvent(requisites));
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
            ActivationDateTime = DateTime.UtcNow;

            _events.Add(GetAccountActivatedEvent(ActivationDateTime.Value));
        }

        private AccountActivated GetAccountActivatedEvent(DateTime activationDateTime)
        {
            return new AccountActivated
            {
                ActivatedAt = activationDateTime,
                AccountId = AccountId
            };
        }

        private static AccountRequisitesAdded GetAccountRequisitesAddedEvent(AccountRequisites requisites)
        {
            return new AccountRequisitesAdded
            {
                CreatedAt = requisites.CreatedAt,
                Address = requisites.NaturalId.Address,
                BlockchainId = requisites.NaturalId.BlockchainId,
                Tag = requisites.NaturalId.Tag,
                TagType = requisites.NaturalId.TagType,
                AccountId = requisites.AccountId,
                AccountRequisitesId = requisites.Id
            };
        }
    }
}
