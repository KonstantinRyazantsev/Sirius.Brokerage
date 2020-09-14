using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.VaultAgent.ApiClient;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class AddBlockchainToBrokerAccountConsumer : IConsumer<AddBlockchainToBrokerAccount>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly ILogger<AddBlockchainToBrokerAccountConsumer> _logger;

        public AddBlockchainToBrokerAccountConsumer(ILoggerFactory loggerFactory,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IVaultAgentClient vaultAgentClient)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<AddBlockchainToBrokerAccountConsumer>();
            _unitOfWorkManager = unitOfWorkManager;
            _vaultAgentClient = vaultAgentClient;
        }

        public async Task Consume(ConsumeContext<AddBlockchainToBrokerAccount> context)
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
            await using var unitOfWork = await _unitOfWorkManager
                .Begin($"BrokerAccounts:AddBlockchainToBrokerAccount:{command.BrokerAccountId}-{idempotencyIdPart}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(command.BrokerAccountId);
                var expectedAccountsCount = await unitOfWork.Accounts.GetCountForBrokerId(brokerAccount.Id);

                await brokerAccount.FinalizeBlockchainAdd(
                    _loggerFactory.CreateLogger<BrokerAccount>(),
                    _vaultAgentClient,
                    command.BlockchainIds,
                    expectedAccountsCount);

                unitOfWork.Outbox.Send(new AddBlockchainsToAccounts()
                {
                    ExpectedAccountsCount = expectedAccountsCount,
                    BlockchainIds = command.BlockchainIds,
                    BrokerAccountId = brokerAccount.Id
                });

                foreach (var evt in brokerAccount.Events)
                {
                    unitOfWork.Outbox.Publish(evt);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
