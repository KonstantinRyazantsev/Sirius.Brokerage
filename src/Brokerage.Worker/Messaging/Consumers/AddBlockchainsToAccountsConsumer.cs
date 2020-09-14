using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
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
    public class AddBlockchainsToAccountsConsumer : IConsumer<AddBlockchainsToAccounts>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly ILogger<AddBlockchainsToAccountsConsumer> _logger;

        public AddBlockchainsToAccountsConsumer(ILoggerFactory loggerFactory,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IVaultAgentClient vaultAgentClient,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            IBlockchainsRepository blockchainsRepository)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<AddBlockchainsToAccountsConsumer>();
            _unitOfWorkManager = unitOfWorkManager;
            _vaultAgentClient = vaultAgentClient;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
            _blockchainsRepository = blockchainsRepository;
        }

        public async Task Consume(ConsumeContext<AddBlockchainsToAccounts> context)
        {
            var command = context.Message;

            if (command.BlockchainIds == null ||
                !command.BlockchainIds.Any())
            {
                _logger.LogWarning("There are no blockchains to add {@context}", command);
                return;
            }

            var blockchainIds = command.BlockchainIds.OrderBy(x => x);
            var idempotencyIdPart = string.Join(", ", blockchainIds);
            var blockchains = await _blockchainsRepository.GetByIds(command.BlockchainIds);

            await using var unitOfWork = await _unitOfWorkManager
                .Begin($"Accounts:AddBlockchainsToAccounts:{command.BrokerAccountId}-{idempotencyIdPart}-{command.Cursor}");
            if (!unitOfWork.Outbox.IsClosed)
            {
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(command.BrokerAccountId);
                var limit = 10;
                var accountsToProcess = await unitOfWork.Accounts
                    .GetForBrokerAccount(command.BrokerAccountId, command.Cursor, limit: limit);

                foreach (var account in accountsToProcess)
                {
                    await account.AddBlockchains(
                        _loggerFactory.CreateLogger<Account>(),
                        brokerAccount,
                        _vaultAgentClient,
                        _destinationTagGeneratorFactory,
                        blockchains,
                        command.ExpectedAccountsCount);

                    foreach (var evt in account.Commands)
                    {
                        unitOfWork.Outbox.Send(evt);
                    }

                    foreach (var evt in account.Events)
                    {
                        unitOfWork.Outbox.Publish(evt);
                    }
                }

                if (accountsToProcess.Count >= limit)
                {
                    unitOfWork.Outbox.Send(new AddBlockchainsToAccounts()
                    {
                        BlockchainIds = command.BlockchainIds,
                        Cursor = accountsToProcess.Last().Id,
                        BrokerAccountId = command.BrokerAccountId,
                        ExpectedAccountsCount = command.ExpectedAccountsCount
                    });
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);


        }
    }
}
