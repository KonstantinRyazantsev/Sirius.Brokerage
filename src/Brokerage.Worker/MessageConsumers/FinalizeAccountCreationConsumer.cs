using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
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
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOutboxManager _outboxManager;

        public FinalizeAccountCreationConsumer(
            ILoggerFactory loggerFactory,
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IAccountRequisitesRepository accountRequisitesRepository,
            IAccountsRepository accountsRepository,
            IOutboxManager outboxManager)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _accountRequisitesRepository = accountRequisitesRepository;
            _accountsRepository = accountsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var command = context.Message;
            var account = await _accountsRepository.GetAsync(command.AccountId);

            try
            {
                await account.FinalizeCreation(
                    _loggerFactory.CreateLogger<Account>(),
                    _blockchainsRepository,
                    _accountRequisitesRepository,
                    _vaultAgentClient,
                    _outboxManager);
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
