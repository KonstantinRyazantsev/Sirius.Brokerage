using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class FinalizeBrokerAccountCreationConsumer : IConsumer<FinalizeBrokerAccountCreation>
    {
        private readonly ILogger<FinalizeBrokerAccountCreationConsumer> _logger;
        private readonly IBlockchainReadModelRepository _blockchainReadModelRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountRepository _brokerAccountRepository;

        public FinalizeBrokerAccountCreationConsumer(
            ILogger<FinalizeBrokerAccountCreationConsumer> logger,
            IBlockchainReadModelRepository blockchainReadModelRepository,
            IVaultAgentClient vaultAgentClient,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountRepository brokerAccountRepository)
        {
            _logger = logger;
            _blockchainReadModelRepository = blockchainReadModelRepository;
            _vaultAgentClient = vaultAgentClient;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountRepository = brokerAccountRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            var message = context.Message;
            BlockchainId cursor = null;

            var brokerAccount = await _brokerAccountRepository.GetAsync(message.BrokerAccountId);

            if (brokerAccount == null)
            {
                _logger.LogInformation("FinalizeBrokerAccountCreation command has no related broker account {@message}", message);
            }
            else
            {
                if (brokerAccount.State == BrokerAccountState.Creating)
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
                            var newRequisites = BrokerAccountRequisites.Create(
                                requestIdForCreation,
                                message.BrokerAccountId,
                                blockchain.BlockchainId);

                            var requisites = await _brokerAccountRequisitesRepository.AddOrGetAsync(newRequisites);

                            if (requisites.Address != null)
                                continue;

                            var requestIdForGeneration = $"{message.RequestId}_{requisites.Id}";

                            var response = await _vaultAgentClient.Wallets.GenerateAsync(new GenerateRequest()
                            {
                                BlockchainId = blockchain.BlockchainId.Value,
                                TenantId = message.TenantId,
                                RequestId = requestIdForGeneration
                            });

                            if (response.BodyCase == GenerateResponse.BodyOneofCase.Error)
                            {
                                _logger.LogWarning("FinalizeBrokerAccountCreation command has been failed {@message}" +
                                                   "error response from vault agent {@response}", message, response);

                                throw new InvalidOperationException($"FinalizeBrokerAccountCreation command " +
                                                                    $"has been failed with {response.Error.ErrorMessage}");
                            }

                            requisites.Address = response.Response.Address;
                            await _brokerAccountRequisitesRepository.UpdateAsync(requisites);
                        }

                    } while (true);

                    brokerAccount.Activate();

                    await _brokerAccountRepository.UpdateAsync(brokerAccount);
                }

                await context.Publish(new BrokerAccountActivated()
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    ActivationDate = brokerAccount.ActivationDateTime.Value,
                    BrokerAccountId = brokerAccount.BrokerAccountId
                });
            }

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@message}", message);

            await Task.CompletedTask;
        }
    }
}
