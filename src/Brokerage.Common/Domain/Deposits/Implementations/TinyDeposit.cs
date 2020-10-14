using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Domain.Accounts;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Implementations
{
    public class TinyDeposit : Deposit
    {
        protected TinyDeposit(
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
                DepositType.TinyDeposit)
        {
        }

        public override void Confirm(TransactionConfirmed tx)
        {
            if (IsBrokerDeposit)
            {
                throw new InvalidOperationException("Can't confirm a broker deposit as a tiny deposit");
            }

            SwitchState(new[] { DepositState.DetectedTiny }, DepositState.ConfirmedTiny);

            TransactionInfo = TransactionInfo.UpdateRequiredConfirmationsCount(tx.RequiredConfirmationsCount);
            UpdatedAt = DateTime.UtcNow;

            AddDepositUpdatedEvent();
        }

        public MinDepositResidual GetResidual(AccountDetails accountDetails)
        {
            return MinDepositResidual.Create(this.Id, this.Unit.Amount, accountDetails.NaturalId, this.Unit.AssetId);
        }

        public override void Complete(IReadOnlyCollection<Unit> fees)
        {
            SwitchState(new[] { DepositState.ConfirmedTiny }, DepositState.CompletedTiny);

            Fees = fees;
            UpdatedAt = DateTime.UtcNow;

            AddDepositUpdatedEvent();
        }

        public static TinyDeposit Create(
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

            var deposit = new TinyDeposit(
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
        public static TinyDeposit Restore(
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
            return new TinyDeposit(
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
