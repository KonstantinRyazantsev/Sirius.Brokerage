using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.BrokerAccounts;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Withdrawals;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class OperationSentConsumer : IConsumer<OperationSent>
    {
        private readonly ILogger<OperationSentConsumer> _logger;
        private readonly OperationProcessingContextBuilder _processingContextBuilder;
        private readonly IProcessorsFactory _processorsFactory;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IWithdrawalsRepository _withdrawalsRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IOperationsRepository _operationsRepository;

        public OperationSentConsumer(ILogger<OperationSentConsumer> logger,
            OperationProcessingContextBuilder processingContextBuilder,
            IProcessorsFactory processorsFactory,
            IDepositsRepository depositsRepository,
            IWithdrawalsRepository withdrawalsRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IOperationsRepository operationsRepository)
        {
            _logger = logger;
            _processingContextBuilder = processingContextBuilder;
            _processorsFactory = processorsFactory;
            _depositsRepository = depositsRepository;
            _withdrawalsRepository = withdrawalsRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _operationsRepository = operationsRepository;
        }

        public async Task Consume(ConsumeContext<OperationSent> context)
        {
            var evt = context.Message;

            var processingContext = await _processingContextBuilder.Build(evt.OperationId);
            
            if (processingContext.IsEmpty)
            {
                _logger.LogInformation("There is nothing to process in the operation {@context}", evt);

                return;
            }

            foreach (var processor in _processorsFactory.GetSentOperationProcessors())
            {
                await processor.Process(evt, processingContext);
            }

            var updatedDeposits = processingContext.Deposits.Where(x => x.Events.Any()).ToArray();
            var updatedWithdrawals = processingContext.Withdrawals.Where(x => x.Events.Any()).ToArray();
            var updatedBrokerAccountBalances = processingContext.BrokerAccountBalances.Values.Where(x => x.Events.Any()).ToArray();
            var operation = processingContext.Operation;

            await Task.WhenAll(
                _depositsRepository.Save(updatedDeposits),
                _withdrawalsRepository.Update(updatedWithdrawals),
                _brokerAccountsBalancesRepository.Save(updatedBrokerAccountBalances),
                _operationsRepository.Update(operation));
            
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
