using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.AccountRequisites;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class FinalizeAccountCreationConsumer : IConsumer<FinalizeAccountCreation>
    {
        private readonly ILogger<FinalizeAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IAccountsRepository accountsRepository;

        public FinalizeAccountCreationConsumer(
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IAccountRequisitesRepository accountRequisitesRepository,
            IAccountsRepository accountsRepository)
        {
            _logger = logger;
            this.blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _accountRequisitesRepository = accountRequisitesRepository;
            this.accountsRepository = accountsRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var command = context.Message;
            var account = await accountsRepository.GetAsync(command.AccountId);
            var accountRequisites = new List<AccountRequisites>(20);

            if (account.State == AccountState.Creating)
            {
                BlockchainId cursor = null;

                do
                {
                    var blockchains = await blockchainsRepository.GetAllAsync(cursor, 100);

                    if (!blockchains.Any())
                        break;

                    cursor = blockchains.Last().BlockchainId;

                    foreach (var blockchain in blockchains)
                    {
                        var requestIdForCreation = $"{command.RequestId}_{blockchain.BlockchainId}";
                        var newRequisites = AccountRequisites.Create(
                            requestIdForCreation,
                            command.AccountId,
                            blockchain.BlockchainId,
                            null);

                        var requisites = await _accountRequisitesRepository.AddOrGetAsync(newRequisites);

                        if (requisites.Address != null)
                            continue;

                        var requestIdForGeneration = $"{command.RequestId}_{requisites.AccountRequisitesId}";

                        var response = await _vaultAgentClient.Wallets.GenerateAsync(new GenerateRequest
                        {
                            BlockchainId = blockchain.BlockchainId,
                            RequestId = requestIdForGeneration
                        });

                        if (response.BodyCase == GenerateResponse.BodyOneofCase.Error)
                        {
                            _logger.LogWarning("FinalizeAccountCreation command has been failed {@message}" +
                                               "error response from vault agent {@response}", command, response);

                            throw new InvalidOperationException($"FinalizeAccountCreation command " +
                                                                $"has been failed with {response.Error.ErrorMessage}");
                        }

                        requisites.Address = response.Response.Address;
                        await _accountRequisitesRepository.UpdateAsync(requisites);

                        accountRequisites.Add(requisites);
                    }

                } while (true);

                account.Activate();

                await accountsRepository.UpdateAsync(account);
            }

            if (accountRequisites.Count == 0)
            {
                long? requisitesCursor = null;

                do
                {
                    var result = await
                        _accountRequisitesRepository.GetByAccountAsync(account.AccountId, 100, requisitesCursor, true);

                    if (!result.Any())
                        break;

                    accountRequisites.AddRange(result);
                    requisitesCursor = result.Last()?.AccountRequisitesId;

                } while (requisitesCursor != null);
            }

            foreach (var requisites in accountRequisites)
            {
                await context.Publish(new AccountRequisitesAdded
                {
                    CreationDateTime = DateTime.UtcNow,
                    Address = requisites.Address,
                    BlockchainId = requisites.BlockchainId,
                    Tag = requisites.Tag,
                    TagType = requisites.TagType.HasValue ?
                        requisites.TagType.Value switch
                        {
                            Swisschain.Sirius.Sdk.Primitives.DestinationTagType.Number => TagType.Number,
                            Swisschain.Sirius.Sdk.Primitives.DestinationTagType.Text=> TagType.Text,

                            _ => throw  new ArgumentOutOfRangeException(nameof(requisites.TagType), requisites.TagType, null)
                        } : (TagType?)null ,
                    AccountId = requisites.AccountId,
                    AccountRequisitesId = requisites.AccountRequisitesId
                });
            }

            await context.Publish(new AccountActivated
            {
                // ReSharper disable once PossibleInvalidOperationException
                ActivationDate = account.ActivationDateTime.Value,
                AccountId = account.AccountId
            });

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@context}", command);
        }
    }
}
