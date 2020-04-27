using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;

namespace Brokerage.Worker.MessageConsumers
{
    public class FinalizeBrokerAccountCreationConsumer : IConsumer<FinalizeBrokerAccountCreation>
    {
        private readonly ILogger<FinalizeBrokerAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IOutboxManager _outboxManager;

        public FinalizeBrokerAccountCreationConsumer(
            ILogger<FinalizeBrokerAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsRepository brokerAccountsRepository,
            IOutboxManager outboxManager)
        {
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsRepository = brokerAccountsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            // TODO: Use outbox correctly here

            var message = context.Message;
            var brokerAccount = await _brokerAccountsRepository.GetAsync(message.BrokerAccountId);
            var brokerAccountRequisites = new List<BrokerAccountRequisites>(20);

            if (brokerAccount.State == BrokerAccountState.Creating)
            {
                string cursor = null;

                do
                {
                    var blockchains = await _blockchainsRepository.GetAllAsync(cursor, 100);

                    if (!blockchains.Any())
                        break;

                    cursor = blockchains.Last().Id;

                    foreach (var blockchain in blockchains)
                    {
                        var outbox = await _outboxManager.Open(
                            $"BrokerAccountRequisites:Create:{message.RequestId}_{blockchain.Id}",
                            () => _brokerAccountRequisitesRepository.GetNextIdAsync());

                        var requestIdForGeneration = $"{message.RequestId}_{outbox.AggregateId}";

                        var response = await _vaultAgentClient.Wallets.GenerateAsync(new GenerateWalletRequest
                        {
                            BlockchainId = blockchain.Id,
                            TenantId = message.TenantId,
                            RequestId = requestIdForGeneration
                        });

                        if (response.BodyCase == GenerateWalletResponse.BodyOneofCase.Error)
                        {
                            _logger.LogWarning("FinalizeBrokerAccountCreation command has been failed {@message}" +
                                               "error response from vault agent {@response}",
                                message,
                                response);

                            throw new InvalidOperationException($"FinalizeBrokerAccountCreation command " +
                                                                $"has been failed with {response.Error.ErrorMessage}");
                        }

                        var requisites = BrokerAccountRequisites.Create(
                            outbox.AggregateId,
                            new BrokerAccountRequisitesId(blockchain.Id, response.Response.Address),
                            message.TenantId,
                            message.BrokerAccountId);

                        await _brokerAccountRequisitesRepository.AddOrIgnoreAsync(requisites);

                        brokerAccountRequisites.Add(requisites);
                    }

                } while (true);

                brokerAccount.Activate();

                await _brokerAccountsRepository.UpdateAsync(brokerAccount);
            }

            if (brokerAccountRequisites.Count == 0)
            {
                long? requisitesCursor = null;

                do
                {
                    var result = await _brokerAccountRequisitesRepository.GetByBrokerAccountAsync(
                        brokerAccount.BrokerAccountId,
                        1000,
                        requisitesCursor);

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
                    CreatedAt = requisites.CreatedAt,
                    Address = requisites.NaturalId.Address,
                    BlockchainId = requisites.NaturalId.BlockchainId,
                    BrokerAccountId = requisites.BrokerAccountId,
                    BrokerAccountRequisitesId = requisites.Id
                });
            }

            await context.Publish(new BrokerAccountActivated
            {
                // ReSharper disable once PossibleInvalidOperationException
                UpdatedAt = brokerAccount.UpdatedAt,
                BrokerAccountId = brokerAccount.BrokerAccountId
            });

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@context}", message);
        }
    }
}
