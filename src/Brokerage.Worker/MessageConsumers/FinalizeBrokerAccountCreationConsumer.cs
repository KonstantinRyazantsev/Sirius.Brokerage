﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using MassTransit;
using Microsoft.Extensions.Logging;
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

        public FinalizeBrokerAccountCreationConsumer(
            ILogger<FinalizeBrokerAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IBrokerAccountsRepository brokerAccountsRepository)
        {
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
            _brokerAccountsRepository = brokerAccountsRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            // TODO: Use outbox correctly here

            var message = context.Message;
            var brokerAccount = await _brokerAccountsRepository.GetAsync(message.BrokerAccountId);

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
                        var requestIdForGeneration = $"{message.RequestId}_{blockchain.Id}";

                        var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(new RequesterContext()
                        {
                            AggregateId = brokerAccount.Id,
                            AggregateType = AggregateType.BrokerAccount
                        });

                        var response = await _vaultAgentClient.Wallets.GenerateAsync(new GenerateWalletRequest
                        {
                            BlockchainId = blockchain.Id,
                            TenantId = message.TenantId,
                            RequestId = requestIdForGeneration,
                            VaultId = brokerAccount.VaultId,
                            Component = nameof(Brokerage),
                            Context = requesterContext
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
                    }

                } while (true);
            }

            _logger.LogInformation("FinalizeBrokerAccountCreation command has been processed {@context}", message);
        }
    }
}
