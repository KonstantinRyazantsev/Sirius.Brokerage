using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.AccountRequisites;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccountRequisites;
using Brokerage.Common.Domain.BrokerAccounts;
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
        private readonly IBlockchainReadModelRepository _blockchainReadModelRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IAccountRepository _accountRepository;

        public FinalizeAccountCreationConsumer(
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainReadModelRepository blockchainReadModelRepository,
            IVaultAgentClient vaultAgentClient,
            IAccountRequisitesRepository accountRequisitesRepository,
            IAccountRepository accountRepository)
        {
            _logger = logger;
            _blockchainReadModelRepository = blockchainReadModelRepository;
            _vaultAgentClient = vaultAgentClient;
            _accountRequisitesRepository = accountRequisitesRepository;
            _accountRepository = accountRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var message = context.Message;
            BlockchainId cursor = null;
            var account = await _accountRepository.GetAsync(message.AccountId);

            if (account == null)
            {
                _logger.LogInformation("FinalizeBrokerAccountCreation command has no related account {@message}", message);
            }
            else
            {
                if (account.AccountState == AccountState.Creating)
                {
                    do
                    {
                        var blockchains = await _blockchainReadModelRepository.GetManyAsync(cursor, 100);

                        if (!blockchains.Any())
                            break;

                        cursor = blockchains.Last().BlockchainId;

                        foreach (var blockchain in blockchains)
                        {
                            var requestIdForCreation = $"{message.RequestId}_{blockchain.BlockchainId}";
                            var newRequisites = AccountRequisites.Create(
                                requestIdForCreation,
                                message.AccountId,
                                blockchain.BlockchainId,
                                null);

                            var requisites = await _accountRequisitesRepository.AddOrGetAsync(newRequisites);

                            if (requisites.Address != null)
                                continue;

                            var requestIdForGeneration = $"{message.RequestId}_{requisites.AccountRequisitesId}";

                            var response = await _vaultAgentClient.Wallets.GenerateAsync(new GenerateRequest()
                            {
                                BlockchainId = blockchain.BlockchainId.Value,
                                RequestId = requestIdForGeneration
                            });

                            if (response.BodyCase == GenerateResponse.BodyOneofCase.Error)
                            {
                                _logger.LogWarning("FinalizeAccountCreation command has been failed {@message}" +
                                                   "error response from vault agent {@response}", message, response);

                                throw new InvalidOperationException($"FinalizeAccountCreation command " +
                                                                    $"has been failed with {response.Error.ErrorMessage}");
                            }

                            requisites.Address = response.Response.Address;
                            await _accountRequisitesRepository.UpdateAsync(requisites);
                        }

                    } while (true);

                    account.Activate();

                    await _accountRepository.UpdateAsync(account);
                }

                await context.Publish(new AccountActivated()
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    ActivationDate = account.ActivationDateTime.Value,
                    AccountId = account.AccountId
                });
            }

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@message}", message);

            await Task.CompletedTask;
        }
    }
}
