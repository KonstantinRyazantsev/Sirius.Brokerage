using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.ApiClient;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class FinalizeAccountCreationConsumer : IConsumer<FinalizeAccountCreation>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<FinalizeAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;
        private readonly ISendEndpointProvider _sendEndpoint;

        public FinalizeAccountCreationConsumer(
            ILoggerFactory loggerFactory,
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IAccountsRepository accountsRepository,
            IBrokerAccountsRepository brokerAccountsRepository,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            ISendEndpointProvider sendEndpoint)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _accountsRepository = accountsRepository;
            _brokerAccountsRepository = brokerAccountsRepository;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
            _sendEndpoint = sendEndpoint;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var command = context.Message;
            var account = await _accountsRepository.GetAsync(command.AccountId);
            var brokerAccount = await _brokerAccountsRepository.GetAsync(account.BrokerAccountId);

            await account.FinalizeCreation(
                _loggerFactory.CreateLogger<Account>(),
                brokerAccount,
                _blockchainsRepository,
                _vaultAgentClient,
                _destinationTagGeneratorFactory,
                _sendEndpoint);

            await _accountsRepository.UpdateAsync(account);

            foreach (var evt in account.Events)
            {
                await context.Publish(evt);
            }
        }
    }
}
