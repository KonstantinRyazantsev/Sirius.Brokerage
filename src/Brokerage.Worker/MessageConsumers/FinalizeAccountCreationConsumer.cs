using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.ApiClient;

namespace Brokerage.Worker.MessageConsumers
{
    public class FinalizeAccountCreationConsumer : IConsumer<FinalizeAccountCreation>
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<FinalizeAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IAccountsRepository accountsRepository;

        public FinalizeAccountCreationConsumer(
            ILoggerFactory loggerFactory,
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IAccountRequisitesRepository accountRequisitesRepository,
            IAccountsRepository accountsRepository)
        {
            this.loggerFactory = loggerFactory;
            _logger = logger;
            this.blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _accountRequisitesRepository = accountRequisitesRepository;
            this.accountsRepository = accountsRepository;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var command = context.Message;
            var account = await accountsRepository.GetAsync(command.AccountId);

            try
            {
                await account.FinalizeCreation(
                    loggerFactory.CreateLogger<Account>(),
                    blockchainsRepository,
                    _accountRequisitesRepository,
                    _vaultAgentClient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account finalization has been failed {@context}", command);

                throw;
            }

            await accountsRepository.UpdateAsync(account);

            foreach (var evt in account.Events)
            {
                await context.Publish(evt);
            }

            _logger.LogInformation("Account finalization has been complete {@context}", command);
        }
    }
}
