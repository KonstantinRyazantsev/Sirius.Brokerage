using System.Threading.Tasks;
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
        private readonly IOperationsExecutor _operationsExecutor;

        public ExecuteWithdrawalConsumer(
            ILogger<ExecuteWithdrawalConsumer> logger,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IOperationsExecutor operationsExecutor)
        {
            _logger = logger;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _operationsExecutor = operationsExecutor;
        }

        public async Task Consume(ConsumeContext<ExecuteWithdrawal> context)
        {
            var evt = context.Message;

            var withdrawal = await _withdrawalRepository.GetAsync(evt.WithdrawalId);

            await withdrawal.Execute(_brokerAccountRequisitesRepository, _operationsExecutor);

            await _withdrawalRepository.SaveAsync(withdrawal);

            foreach (var @event in withdrawal.Events)
            {
                await context.Publish(@event);
            }

            _logger.LogInformation("ExecuteWithdrawal has been processed {@context}", evt);

            await Task.CompletedTask;
        }
    }
}
