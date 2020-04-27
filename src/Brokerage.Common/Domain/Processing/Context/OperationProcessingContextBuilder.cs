﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Withdrawals;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class OperationProcessingContextBuilder
    {
        private readonly IOperationsRepository _operationsRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;

        public OperationProcessingContextBuilder(IOperationsRepository operationsRepository,
            IDepositsRepository depositsRepository,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository)
        {
            _operationsRepository = operationsRepository;
            _depositsRepository = depositsRepository;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
        }

        public async Task<OperationProcessingContext> Build(long operationId)
        {
            var operation = await _operationsRepository.GetOrDefault(operationId);

            if (operation == null)
            {
                return OperationProcessingContext.Empty;
            }

            switch (operation.Type)
            {
                case OperationType.DepositConsolidation:
                {
                    var deposit = await _depositsRepository.GetByConsolidationIdOrDefaultAsync(operationId);
                    var brokerAccountBalances = await _brokerAccountsBalancesRepository.GetAsync(
                        new BrokerAccountBalancesId(deposit.BrokerAccountId, deposit.Unit.AssetId));

                    return new OperationProcessingContext(
                        operation,
                        new[] {deposit},
                        Array.Empty<Withdrawal>(),
                        new Dictionary<BrokerAccountBalancesId, BrokerAccountBalances>
                        {
                            [brokerAccountBalances.NaturalId] = brokerAccountBalances
                        });
                }

                // TODO:
                //case OperationType.DepositProvisioning:
                //    break;

                case OperationType.Withdrawal:
                {
                    var withdrawal = await _withdrawalRepository.GetByOperationIdOrDefaultAsync(operationId);
                    var brokerAccountBalances = await _brokerAccountsBalancesRepository.GetAsync(
                        new BrokerAccountBalancesId(withdrawal.BrokerAccountId, withdrawal.Unit.AssetId));

                    return new OperationProcessingContext(
                        operation,
                        Array.Empty<Deposit>(),
                        new[] {withdrawal},
                        new Dictionary<BrokerAccountBalancesId, BrokerAccountBalances>
                        {
                            [brokerAccountBalances.NaturalId] = brokerAccountBalances
                        });
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation.Type), operation.Type, "");
            }
        }
    }
}