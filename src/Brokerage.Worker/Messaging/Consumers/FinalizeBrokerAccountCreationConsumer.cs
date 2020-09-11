using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Blockchains;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.VaultAgent.ApiClient;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class FinalizeBrokerAccountCreationConsumer : IConsumer<FinalizeBrokerAccountCreation>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IVaultAgentClient _vaultAgentClient;

        public FinalizeBrokerAccountCreationConsumer(ILoggerFactory loggerFactory,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IVaultAgentClient vaultAgentClient)
        {
            _loggerFactory = loggerFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _vaultAgentClient = vaultAgentClient;
        }

        public async Task Consume(ConsumeContext<FinalizeBrokerAccountCreation> context)
        {
            var command = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"BrokerAccounts:FinalizeCreation:{command.BrokerAccountId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(command.BrokerAccountId);

                await brokerAccount.FinalizeCreation(
                    _loggerFactory.CreateLogger<BrokerAccount>(),
                    _vaultAgentClient);

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
