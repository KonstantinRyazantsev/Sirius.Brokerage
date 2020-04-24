using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Withdrawals;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Brokerage.Worker.MessageConsumers
{
    public class ExecuteWithdrawalConsumer : IConsumer<ExecuteWithdrawal>
    {
        private readonly ILogger<ExecuteWithdrawalConsumer> _logger;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IOperationsExecutor _operationsExecutor;

        public ExecuteWithdrawalConsumer(
            ILogger<ExecuteWithdrawalConsumer> logger,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IOperationsExecutor operationsExecutor)
        {
            _logger = logger;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _operationsExecutor = operationsExecutor;
        }

        public async Task Consume(ConsumeContext<ExecuteWithdrawal> context)
        {
            // TODO: Idempotency

            var evt = context.Message;

            var withdrawal = await _withdrawalRepository.GetAsync(evt.WithdrawalId);
            
            var executionTask = withdrawal.Execute(
                _brokerAccountRequisitesRepository, 
                _operationsExecutor);

            var brokerAccountBalances = await _brokerAccountsBalancesRepository.GetAsync(
                new BrokerAccountBalancesId(withdrawal.BrokerAccountId, withdrawal.Unit.AssetId));

            await executionTask;

            await _withdrawalRepository.SaveAsync(new[] {withdrawal});

            await _brokerAccountsBalancesRepository.SaveAsync(
                $"{BalanceChangingReason.OperationStarted}_{withdrawal.OperationId}",
                new[] {brokerAccountBalances});

            foreach (var @event in withdrawal.Events)
            {
                await context.Publish(@event);
            }

            foreach (var @event in brokerAccountBalances.Events)
            {
                await context.Publish(@event);
            }

            _logger.LogInformation("ExecuteWithdrawal has been processed {@context}", evt);

            await Task.CompletedTask;
        }
    }
}
