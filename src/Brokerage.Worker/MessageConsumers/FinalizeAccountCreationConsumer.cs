using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.VaultAgent.ApiClient;

namespace Brokerage.Worker.MessageConsumers
{
    public class FinalizeAccountCreationConsumer : IConsumer<FinalizeAccountCreation>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<FinalizeAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IAccountDetailsRepository _accountDetailsRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOutboxManager _outboxManager;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;
        private readonly ISendEndpointProvider _sendEndpoint;

        public FinalizeAccountCreationConsumer(
            ILoggerFactory loggerFactory,
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IAccountDetailsRepository accountDetailsRepository,
            IAccountsRepository accountsRepository,
            IOutboxManager outboxManager,
            IBrokerAccountsRepository brokerAccountsRepository,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            ISendEndpointProvider sendEndpoint)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _accountDetailsRepository = accountDetailsRepository;
            _accountsRepository = accountsRepository;
            _outboxManager = outboxManager;
            _brokerAccountsRepository = brokerAccountsRepository;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
            _sendEndpoint = sendEndpoint;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var command = context.Message;
            var account = await _accountsRepository.GetAsync(command.AccountId);
            var brokerAccount = await _brokerAccountsRepository.GetAsync(account.BrokerAccountId);

            try
            {
                await account.FinalizeCreation(
                    _loggerFactory.CreateLogger<Account>(),
                    brokerAccount,
                    _blockchainsRepository,
                    _vaultAgentClient,
                    _destinationTagGeneratorFactory,
                    _sendEndpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account finalization has been failed {@context}", command);

                throw;
            }

            await _accountsRepository.UpdateAsync(account);

            foreach (var evt in account.Events)
            {
                await context.Publish(evt);
            }

            _logger.LogInformation("Account finalization has been complete {@context}", command);
        }
    }
}
