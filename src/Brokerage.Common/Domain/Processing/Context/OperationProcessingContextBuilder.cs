using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Domain.Deposits.Implementations;
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
            IWithdrawalsRepository withdrawalsRepository,
            IMinDepositResidualsRepository minDepositResidualsRepository)
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
                    var deposits = (await depositsRepository.Search(
                        blockchainId: null,
                        transactionId: null,
                        consolidationOperationId: operation.Id))
                        .Cast<RegularDeposit>()
                        .ToArray();
                    var brokerAccountIds = deposits
                        .Select(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                        .ToHashSet();
                    var brokerAccountBalances = (await brokerAccountsBalancesRepository.GetAnyOf(brokerAccountIds))
                        .ToDictionary(x => x.NaturalId);
                    var minDepositResiduals = await minDepositResidualsRepository.GetForConsolidationDeposits(
                        deposits.Select(x => x.Id).ToHashSet());
                    var rawMinDeposits = await depositsRepository.GetAnyFor(
                        minDepositResiduals
                            .Where(x => x.ConsolidationDepositId.HasValue)
                            .Select(x => x.ConsolidationDepositId.Value)
                            .ToHashSet());
                    var minDeposits = rawMinDeposits.Where(x => x is TinyDeposit).Cast<TinyDeposit>().ToArray();

                    return new OperationProcessingContext(
                        operation,
                        deposits,
                        Array.Empty<Withdrawal>(),
                        brokerAccountBalances,
                        minDepositResiduals,
                        minDeposits);
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
                        Array.Empty<RegularDeposit>(),
                        new[] {withdrawal},
                        new Dictionary<BrokerAccountBalancesId, BrokerAccountBalances>
                        {
                            [brokerAccountBalances.NaturalId] = brokerAccountBalances
                        },
                        Array.Empty<MinDepositResidual>(),
                        Array.Empty<TinyDeposit>());
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation.Type), operation.Type, "");
            }
        }
    }
}
