using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.BrokerAccounts;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Withdrawals;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class OperationProcessingContextBuilder
    {
        public async Task<OperationProcessingContext> Build(long operationId,
            IOperationsRepository operationsRepository,
            IDepositsRepository depositsRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IWithdrawalsRepository withdrawalsRepository)
        {
            var operation = await operationsRepository.GetOrDefault(operationId);

            if (operation == null)
            {
                return OperationProcessingContext.Empty;
            }

            switch (operation.Type)
            {
                case OperationType.DepositConsolidation:
                {
                    var deposits = await depositsRepository.Search(
                        blockchainId: null,
                        transactionId: null,
                        consolidationOperationId: operation.Id);
                    var brokerAccountIds = deposits
                        .Select(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                        .ToHashSet();
                    var brokerAccountBalances = (await brokerAccountsBalancesRepository.GetAnyOf(brokerAccountIds))
                        .ToDictionary(x => x.NaturalId);

                    return new OperationProcessingContext(
                        operation,
                        deposits,
                        Array.Empty<Withdrawal>(),
                        brokerAccountBalances);
                }

                // TODO:
                //case OperationType.DepositProvisioning:
                //    break;

                case OperationType.Withdrawal:
                {
                    var withdrawal = await withdrawalsRepository.GetByOperationIdOrDefault(operationId);
                    var brokerAccountBalances = await brokerAccountsBalancesRepository.GetAsync(
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
