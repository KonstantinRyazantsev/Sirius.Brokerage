﻿using System;
using System.Collections.Generic;
using System.Linq;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Implementations
{
    public class BrokerDeposit : Deposit
    {
        protected BrokerDeposit(
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
                DepositType.BrokerDeposit)
        {
        }

        public override void Confirm(TransactionConfirmed tx)
        {
            if (!IsBrokerDeposit)
            {
                throw new InvalidOperationException("Can't confirm a regular deposit as a broker deposit");
            }

            SwitchState(new[] { DepositState.Detected }, DepositState.Completed);

            var date = DateTime.UtcNow;

            TransactionInfo = TransactionInfo.UpdateRequiredConfirmationsCount(tx.RequiredConfirmationsCount);
            UpdatedAt = date;

            AddDepositUpdatedEvent();
        }

        public override void Complete(IReadOnlyCollection<Unit> fees)
        {
            SwitchState(new[] { DepositState.ConfirmedTiny }, DepositState.CompletedTiny);

            Fees = fees;
            UpdatedAt = DateTime.UtcNow;

            AddDepositUpdatedEvent();
        }

        public static BrokerDeposit Create(
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

            var deposit = new BrokerDeposit(
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
        public static BrokerDeposit Restore(
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
            return new BrokerDeposit(
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
