using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence;
using MassTransit;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class ExecuteWithdrawalConsumer : IConsumer<ExecuteWithdrawal>
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IOperationsExecutor _operationsExecutor;

        public ExecuteWithdrawalConsumer(IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IOperationsExecutor operationsExecutor)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _operationsExecutor = operationsExecutor;
        }

        public async Task Consume(ConsumeContext<ExecuteWithdrawal> context)
        {
            var command = context.Message;

            await using var unitOfWork = await _unitOfWorkManager.Begin($"Withdrawals:Execute:{command.WithdrawalId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var withdrawal = await unitOfWork.Withdrawals.Get(command.WithdrawalId);

                var executionTask = withdrawal.Execute(
                    unitOfWork.BrokerAccounts,
                    unitOfWork.BrokerAccountDetails, 
                    _operationsExecutor);

                var brokerAccountBalances = await unitOfWork.BrokerAccountBalances.Get(
                    new BrokerAccountBalancesId(withdrawal.BrokerAccountId, withdrawal.Unit.AssetId));

                brokerAccountBalances.ReserveBalance(withdrawal.Unit.Amount);

                await executionTask;

                await Task.WhenAll(
                    unitOfWork.Withdrawals.Update(new[] {withdrawal}),
                    unitOfWork.BrokerAccountBalances.Save(new[] {brokerAccountBalances}));

                foreach (var evt in withdrawal.Events)
                {
                    unitOfWork.Outbox.Publish(evt);
                }

                foreach (var evt in brokerAccountBalances.Events)
                {
                    unitOfWork.Outbox.Publish(evt);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
