using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Withdrawals;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class OperationFailedConsumer : IConsumer<OperationFailed>
    {
        private readonly ILogger<OperationFailedConsumer> _logger;
        private readonly OperationProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;

        public OperationFailedConsumer(ILogger<OperationFailedConsumer> logger,
            OperationProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory,
            IDepositsRepository depositsRepository,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository)
        {
            _logger = logger;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
            _depositsRepository = depositsRepository;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
        }

        public async Task Consume(ConsumeContext<OperationFailed> context)
        {
            var evt = context.Message;

            var processingContext = await _processingContextBuilder.Build(evt.OperationId);
            
            if (processingContext.IsEmpty)
            {
                _logger.LogInformation("There is nothing to process in the operation {@context}", evt);

                return;
            }

            foreach (var processor in _processorsFactory.GetFailedOperationProcessors())
            {
                await processor.Process(evt, processingContext);
            }

            var updatedDeposits = processingContext.Deposits.Where(x => x.Events.Any()).ToArray();
            var updatedWithdrawals = processingContext.Withdrawals.Where(x => x.Events.Any()).ToArray();
            var updatedBrokerAccountBalances = processingContext.BrokerAccountBalances.Values.Where(x => x.Events.Any()).ToArray();

            await Task.WhenAll(
                _depositsRepository.SaveAsync(updatedDeposits),
                _withdrawalRepository.SaveAsync(updatedWithdrawals),
                _brokerAccountsBalancesRepository.SaveAsync(
                    $"{BalanceChangingReason.OperationFailed}_{processingContext.Operation.Id}",
                    updatedBrokerAccountBalances));
            
            foreach (var @event in updatedDeposits.SelectMany(x => x.Events))
            {
                await context.Publish(@event);
            }

            foreach (var @event in updatedWithdrawals.SelectMany(x => x.Events))
            {
                await context.Publish(@event);
            }

            foreach (var @event in updatedBrokerAccountBalances.SelectMany(x => x.Events))
            {
                await context.Publish(@event);
            }
        }
    }
}
