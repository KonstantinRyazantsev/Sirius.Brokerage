using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Implementations
{
    public class RegularDeposit : Deposit
    {
        protected RegularDeposit(
            long id,
            uint version,
            long sequence,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountDetailsId,
            Unit unit,
            long? consolidationOperationId,
            IReadOnlyCollection<Unit> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState state,
            IReadOnlyCollection<DepositSource> sources,
            DateTime createdAt,
            DateTime updatedAt,
            decimal minDepositForConsolidation) :
            base(id, version, sequence,
                tenantId,
                blockchainId,
                brokerAccountId,
                brokerAccountDetailsId,
                accountDetailsId,
                unit,
                consolidationOperationId,
                fees,
                transactionInfo,
                error,
                state,
                sources,
                createdAt,
                updatedAt,
                minDepositForConsolidation,
                DepositType.Regular)
        {
        }

        public void Fail(DepositError depositError)
        {
            SwitchState(new[] { DepositState.Confirmed }, DepositState.Failed);

            UpdatedAt = DateTime.UtcNow;
            Error = depositError;

            AddDepositUpdatedEvent();
        }

        public async Task<Operation> Confirm(
            BrokerAccount brokerAccount,
            BrokerAccountDetails brokerAccountDetails,
            AccountDetails accountDetails,
            TransactionConfirmed tx,
            IOperationsFactory operationsFactory,
            decimal residual,
            Account account)
        {
            SwitchState(new[] { DepositState.Detected, }, DepositState.Confirmed);

            var consolidationAmount = new Unit(
                this.Unit.AssetId,
                this.Unit.Amount + residual);

            var consolidationOperation = await operationsFactory.StartDepositConsolidation(
                TenantId,
                Id,
                accountDetails.NaturalId.Address,
                brokerAccountDetails.NaturalId.Address,
                consolidationAmount,
                tx.BlockNumber,
                brokerAccount.VaultId,
                account.ReferenceId,
                brokerAccount.Id);

            ConsolidationOperationId = consolidationOperation.Id;
            TransactionInfo = TransactionInfo.UpdateRequiredConfirmationsCount(tx.RequiredConfirmationsCount);
            UpdatedAt = DateTime.UtcNow;

            AddDepositUpdatedEvent();

            return consolidationOperation;
        }

        public void CompleteWithDestinationTag(TransactionConfirmed tx)
        {
            SwitchState(new[] { DepositState.Detected }, DepositState.Completed);

            var date = DateTime.UtcNow;

            TransactionInfo = TransactionInfo.UpdateRequiredConfirmationsCount(tx.RequiredConfirmationsCount);
            UpdatedAt = date;

            AddDepositUpdatedEvent();
        }

        public void CompleteWithoutDestinationTag(IReadOnlyCollection<Unit> fees)
        {
            SwitchState(new[] { DepositState.Confirmed }, DepositState.Completed);

            Fees = fees;
            UpdatedAt = DateTime.UtcNow;

            AddDepositUpdatedEvent();
        }

        public static RegularDeposit Create(
            long id,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountDetailsId,
            Unit unit,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<DepositSource> sources,
            decimal minDepositForConsolidation)
        {
            var createdAt = DateTime.UtcNow;
            var state = DepositState.Detected;

            var deposit = new RegularDeposit(
                id,
                default,
                0,
                tenantId,
                blockchainId,
                brokerAccountId,
                brokerAccountDetailsId,
                accountDetailsId,
                unit,
                null,
                Array.Empty<Unit>(),
                transactionInfo,
                null,
                state,
                sources
                    .GroupBy(x => x.Address)
                    .Select(g => new DepositSource(g.Key, g.Sum(x => x.Amount)))
                    .ToArray(),
                createdAt,
                createdAt,
                minDepositForConsolidation);

            deposit.AddDepositUpdatedEvent();

            return deposit;
        }
        public static RegularDeposit Restore(
            long id,
            uint version,
            long sequence,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountDetailsId,
            Unit unit,
            long? consolidationOperationId,
            IReadOnlyCollection<Unit> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState depositState,
            IReadOnlyCollection<DepositSource> sources,
            DateTime createdAt,
            DateTime updatedAt,
            decimal minDepositForConsolidation)
        {
            return new RegularDeposit(
                id,
                version,
                sequence,
                tenantId,
                blockchainId,
                brokerAccountId,
                brokerAccountDetailsId,
                accountDetailsId,
                unit,
                consolidationOperationId,
                fees,
                transactionInfo,
                error,
                depositState,
                sources,
                createdAt,
                updatedAt,
                minDepositForConsolidation);
        }
    }
}
