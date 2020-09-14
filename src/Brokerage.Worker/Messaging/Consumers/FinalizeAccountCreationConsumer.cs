using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Blockchains;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.VaultAgent.ApiClient;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class FinalizeAccountCreationConsumer : IConsumer<FinalizeAccountCreation>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;

        public FinalizeAccountCreationConsumer(
            ILoggerFactory loggerFactory,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IBlockchainsRepository blockchainsRepository,
            IVaultAgentClient vaultAgentClient,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory)
        {
            _loggerFactory = loggerFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _blockchainsRepository = blockchainsRepository;
            _vaultAgentClient = vaultAgentClient;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
        }

        public async Task Consume(ConsumeContext<FinalizeAccountCreation> context)
        {
            var command = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"Accounts:FinalizeCreation:{command.AccountId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var account = await unitOfWork.Accounts.Get(command.AccountId);
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(account.BrokerAccountId);

                await account.FinalizeCreation(
                    _loggerFactory.CreateLogger<Account>(),
                    brokerAccount,
                    _blockchainsRepository,
                    _vaultAgentClient,
                    _destinationTagGeneratorFactory,
                    unitOfWork.Accounts);

                foreach (var evt in account.Commands)
                {
                    unitOfWork.Outbox.Send(evt);
                }

                foreach (var evt in account.Events)
                {
                    unitOfWork.Outbox.Publish(evt);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
