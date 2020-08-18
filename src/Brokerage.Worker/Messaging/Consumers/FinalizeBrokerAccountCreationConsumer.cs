using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccounts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class FinalizeBrokerAccountCreationConsumer : IConsumer<FinalizeBrokerAccountCreation>
    {
        private readonly ILogger<FinalizeBrokerAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;

        public FinalizeBrokerAccountCreationConsumer(
            ILogger<FinalizeBrokerAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IBrokerAccountsRepository brokerAccountsRepository)
        {
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _brokerAccountsRepository = brokerAccountsRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            // TODO: Use outbox correctly here

            var message = context.Message;
            var brokerAccount = await _brokerAccountsRepository.Get(message.BrokerAccountId);
            var blockchainsCount = await _blockchainsRepository.GetCountAsync();

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
                        var requestIdForGeneration = $"Brokerage:BrokerAccountDetails:{message.RequestId}_{blockchain.Id}";

                        var requesterContext = Newtonsoft.Json.JsonConvert.SerializeObject(new WalletGenerationRequesterContext()
                        {
                            AggregateId = brokerAccount.Id,
                            AggregateType = AggregateType.BrokerAccount,
                            ExpectedCount = blockchainsCount
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
        }
    }
}
