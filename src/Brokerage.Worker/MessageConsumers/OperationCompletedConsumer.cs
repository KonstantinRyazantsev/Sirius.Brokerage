using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Withdrawals;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.MessagingContract;
using OperationProcessingContextBuilder = Brokerage.Common.Domain.Processing.Context.OperationProcessingContextBuilder;

namespace Brokerage.Worker.MessageConsumers
{
    public class OperationCompletedConsumer : IConsumer<OperationCompleted>
    {
        private readonly ILogger<OperationCompletedConsumer> _logger;
        private readonly OperationProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IOperationsRepository _operationsRepository;

        public OperationCompletedConsumer(ILogger<OperationCompletedConsumer> logger,
            OperationProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory,
            IDepositsRepository depositsRepository,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IOperationsRepository operationsRepository)
        {
            _logger = logger;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
            _depositsRepository = depositsRepository;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _operationsRepository = operationsRepository;
        }

        public async Task Consume(ConsumeContext<OperationCompleted> context)
        {
            var evt = context.Message;

            var processingContext = await _processingContextBuilder.Build(evt.OperationId);
            var operation = processingContext.Operation;

            if (processingContext.IsEmpty)
            {
                _logger.LogInformation("There is nothing to process in the operation {@context}", evt);

                return;
            }

            foreach (var processor in _processorsFactory.GetCompletedOperationProcessors())
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
                    $"{BalanceChangingReason.OperationCompleted}_{processingContext.Operation.Id}",
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

            operation.AddActualFees(evt.ActualFees);

            await _operationsRepository.UpdateAsync(operation);

            _logger.LogInformation("Operation completion has been processed {@context}", evt);
        }
    }
}
