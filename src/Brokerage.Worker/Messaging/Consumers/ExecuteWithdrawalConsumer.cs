﻿using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Withdrawals;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class ExecuteWithdrawalConsumer : IConsumer<ExecuteWithdrawal>
    {
        private readonly ILogger<ExecuteWithdrawalConsumer> _logger;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IBrokerAccountDetailsRepository _brokerAccountDetailsRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IOperationsExecutor _operationsExecutor;

        public ExecuteWithdrawalConsumer(
            ILogger<ExecuteWithdrawalConsumer> logger,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IBrokerAccountsRepository brokerAccountsRepository,
            IOperationsExecutor operationsExecutor)
        {
            _logger = logger;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _brokerAccountsRepository = brokerAccountsRepository;
            _operationsExecutor = operationsExecutor;
        }

        public async Task Consume(ConsumeContext<ExecuteWithdrawal> context)
        {
            // TODO: Idempotency

            var evt = context.Message;

            var withdrawal = await _withdrawalRepository.GetAsync(evt.WithdrawalId);
            
            var executionTask = withdrawal.Execute(
                _brokerAccountsRepository,
                _brokerAccountDetailsRepository, 
                _operationsExecutor);

            var brokerAccountBalances = await _brokerAccountsBalancesRepository.GetAsync(
                new BrokerAccountBalancesId(withdrawal.BrokerAccountId, withdrawal.Unit.AssetId));

            brokerAccountBalances.ReserveBalance(withdrawal.Unit.Amount);

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

            await Task.CompletedTask;
        }
    }
}