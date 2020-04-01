using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.Domain.Services;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract;

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
            IWalletsService walletsService)
        {
            if (State == AccountState.Creating)
            {
                await CreateRequisites(logger,
                    blockchainsRepository,
                    requisitesRepository,
                    vaultAgentClient,
                    walletsService);

                Activate();
            }
            else
            {
                long? requisitesCursor = null;

                do
                {
                    var requisitesBatch = await requisitesRepository.GetByAccountAsync(
                        AccountId, 
                        100, 
                        requisitesCursor, 
                        true);

                    if (!requisitesBatch.Any())
                    {
                        break;
                    }

                    _events.AddRange(requisitesBatch.Select(GetAccountRequisitesAddedEvent));

                    requisitesCursor = requisitesBatch.Last().AccountRequisitesId;

                } while (requisitesCursor != null);

                // ReSharper disable once PossibleInvalidOperationException
                _events.Add(GetAccountActivatedEvent(ActivationDateTime.Value));
            }
        }

        private async Task CreateRequisites(
            ILogger<Account> logger,
            IBlockchainsRepository blockchainsRepository, 
            IAccountRequisitesRepository requisitesRepository,
            IVaultAgentClient vaultAgentClient,
            IWalletsService walletsService)
        {
            BlockchainId cursor = null;

            do
            {
                var blockchains = await blockchainsRepository.GetAllAsync(cursor, 100);

                if (!blockchains.Any())
                {
                    break;
                }

                cursor = blockchains.Last().BlockchainId;

                foreach (var blockchain in blockchains)
                {
                    var requestIdForCreation = $"{AccountId}_{blockchain.BlockchainId}";
                    var newRequisites = AccountRequisites.Create(
                        requestIdForCreation,
                        AccountId,
                        BrokerAccountId,
                        blockchain.BlockchainId,
                        null);

                    var requisites = await requisitesRepository.AddOrGetAsync(newRequisites);

                    if (requisites.Address != null)
                        continue;

                    var requestIdForGeneration = $"{AccountId}_{requisites.AccountRequisitesId}";

                    var response = await vaultAgentClient.Wallets.GenerateAsync(new GenerateRequest
                    {
                        BlockchainId = blockchain.BlockchainId,
                        RequestId = requestIdForGeneration
                    });

                    if (response.BodyCase == GenerateResponse.BodyOneofCase.Error)
                    {
                        logger.LogWarning("Wallet generation failed {@context}", response);

                        throw new InvalidOperationException($"Wallet generation has been failed: {response.Error.ErrorMessage}");
                    }

                    requisites.Address = response.Response.Address;

                    await walletsService.ImportWalletAsync(blockchain.BlockchainId, requisites.Address);

                    await requisitesRepository.UpdateAsync(requisites);

                    _events.Add(GetAccountRequisitesAddedEvent(requisites));
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
                ActivationDate = activationDateTime,
                AccountId = AccountId
            };
        }

        private static AccountRequisitesAdded GetAccountRequisitesAddedEvent(AccountRequisites requisites)
        {
            return new AccountRequisitesAdded
            {
                CreationDateTime = requisites.CreationDateTime,
                Address = requisites.Address,
                BlockchainId = requisites.BlockchainId,
                Tag = requisites.Tag,
                TagType = requisites.TagType.HasValue
                    ? requisites.TagType.Value switch
                    {
                        DestinationTagType.Number => TagType.Number,
                        DestinationTagType.Text => TagType.Text,
                        _ => throw new ArgumentOutOfRangeException(nameof(requisites.TagType),
                            requisites.TagType,
                            null)
                    }
                    : (TagType?) null,
                AccountId = requisites.AccountId,
                AccountRequisitesId = requisites.AccountRequisitesId
            };
        }
    }
}
