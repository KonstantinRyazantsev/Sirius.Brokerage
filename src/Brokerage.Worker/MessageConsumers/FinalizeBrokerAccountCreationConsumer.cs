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
        private readonly IBrokerAccountDetailsRepository _brokerAccountDetailsRepository;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IOutboxManager _outboxManager;

        public FinalizeBrokerAccountCreationConsumer(
            ILogger<FinalizeBrokerAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IBrokerAccountsRepository brokerAccountsRepository,
            IOutboxManager outboxManager)
        {
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
            _brokerAccountsRepository = brokerAccountsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            // TODO: Use outbox correctly here

            var message = context.Message;
            var brokerAccount = await _brokerAccountsRepository.GetAsync(message.BrokerAccountId);
            var brokerAccountDetails = new List<BrokerAccountDetails>(20);

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
                            $"BrokerAccountDetails:Create:{message.RequestId}_{blockchain.Id}",
                            () => _brokerAccountDetailsRepository.GetNextIdAsync());

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

                        var details = BrokerAccountDetails.Create(
                            outbox.AggregateId,
                            new BrokerAccountDetailsId(blockchain.Id, response.Response.Address),
                            message.TenantId,
                            message.BrokerAccountId);

                        await _brokerAccountDetailsRepository.AddOrIgnoreAsync(details);

                        brokerAccountDetails.Add(details);
                    }

                } while (true);

                brokerAccount.Activate();

                await _brokerAccountsRepository.UpdateAsync(brokerAccount);
            }

            if (brokerAccountDetails.Count == 0)
            {
                long? detailsCursor = null;

                do
                {
                    var result = await _brokerAccountDetailsRepository.GetByBrokerAccountAsync(
                        brokerAccount.Id,
                        1000,
                        detailsCursor);

                    if (!result.Any())
                        break;

                    brokerAccountDetails.AddRange(result);
                    detailsCursor = result.Last()?.Id;

                } while (detailsCursor != null);
            }

            foreach (var details in brokerAccountDetails)
            {
                await context.Publish(new BrokerAccountDetailsAdded
                {
                    CreatedAt = details.CreatedAt,
                    Address = details.NaturalId.Address,
                    BlockchainId = details.NaturalId.BlockchainId,
                    BrokerAccountId = details.BrokerAccountId,
                    BrokerAccountDetailsId = details.Id
                });
            }

            await context.Publish(new BrokerAccountActivated
            {
                // ReSharper disable once PossibleInvalidOperationException
                UpdatedAt = brokerAccount.UpdatedAt,
                BrokerAccountId = brokerAccount.Id
            });

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@context}", message);
        }
    }
}
