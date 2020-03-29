using System;
using System.Collections.Generic;
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
        private readonly IBlockchainsRepository blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsRepository brokerAccountsRepository;

        public FinalizeBrokerAccountCreationConsumer(
            ILogger<FinalizeBrokerAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsRepository brokerAccountsRepository)
        {
            _logger = logger;
            this.blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            this.brokerAccountsRepository = brokerAccountsRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            var message = context.Message;
            var brokerAccount = await brokerAccountsRepository.GetAsync(message.BrokerAccountId);
            var brokerAccountRequisites = new List<BrokerAccountRequisites>(20);

            if (brokerAccount.State == BrokerAccountState.Creating)
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
                        var requestIdForCreation = $"{message.RequestId}_{blockchain.BlockchainId}";
                        var newRequisites = BrokerAccountRequisites.Create(
                            requestIdForCreation,
                            message.BrokerAccountId,
                            blockchain.BlockchainId);

                        var requisites = await _brokerAccountRequisitesRepository.AddOrGetAsync(newRequisites);

                        if (requisites.Address != null)
                            continue;

                        var requestIdForGeneration = $"{message.RequestId}_{requisites.Id}";

                        var response = await _vaultAgentClient.Wallets.GenerateAsync(new GenerateRequest
                        {
                            BlockchainId = blockchain.BlockchainId,
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

                        brokerAccountRequisites.Add(requisites);
                    }

                } while (true);

                brokerAccount.Activate();

                await brokerAccountsRepository.UpdateAsync(brokerAccount);
            }

            if (brokerAccountRequisites.Count == 0)
            {
                long? requisitesCursor = null;

                do
                {
                    var result = await
                        _brokerAccountRequisitesRepository.SearchAsync(brokerAccount.BrokerAccountId, 100, requisitesCursor, true);

                    if (!result.Any())
                        break;

                    brokerAccountRequisites.AddRange(result);
                    requisitesCursor = result.Last()?.Id;

                } while (requisitesCursor != null);
            }

            foreach (var requisites in brokerAccountRequisites)
            {
                await context.Publish(new BrokerAccountRequisitesAdded
                {
                    CreationDateTime = DateTime.UtcNow,
                    Address = requisites.Address,
                    BlockchainId = requisites.BlockchainId,
                    BrokerAccountId = requisites.BrokerAccountId,
                    BrokerAccountRequisitesId = requisites.Id
                });
            }

            await context.Publish(new BrokerAccountActivated
            {
                // ReSharper disable once PossibleInvalidOperationException
                ActivationDate = brokerAccount.ActivationDateTime.Value,
                BrokerAccountId = brokerAccount.BrokerAccountId
            });

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@context}", message);
        }
    }
}
