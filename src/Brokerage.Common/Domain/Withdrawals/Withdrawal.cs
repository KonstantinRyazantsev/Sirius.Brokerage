using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Withdrawals
{
    public class Withdrawal
    {
        private Withdrawal(
            long id,
            uint version,
            long sequence,
            long brokerAccountId,
            long brokerAccountRequisitesId,
            long? accountId,
            string referenceId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationRequisites destinationRequisites,
            WithdrawalState state,
            TransactionInfo transactionInfo,
            WithdrawalError error,
            long? withdrawalOperationId,
            DateTime createdDateTime,
            DateTime updatedDateTime)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            BrokerAccountRequisitesId = brokerAccountRequisitesId;
            AccountId = accountId;
            ReferenceId = referenceId;
            Unit = unit;
            TenantId = tenantId;
            Fees = fees;
            DestinationRequisites = destinationRequisites;
            State = state;
            TransactionInfo = transactionInfo;
            Error = error;
            WithdrawalOperationId = withdrawalOperationId;
            CreatedDateTime = createdDateTime;
            UpdatedDateTime = updatedDateTime;
            Version = version;
            Sequence = sequence;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; }
        public long BrokerAccountId { get; }

        public long BrokerAccountRequisitesId { get; }

        public long? AccountId { get; }

        public string ReferenceId { get; }

        public Unit Unit { get; }

        public string TenantId { get; }

        public IReadOnlyCollection<Unit> Fees { get; }

        public DestinationRequisites DestinationRequisites { get; }

        public WithdrawalState State { get; private set; }

        public TransactionInfo TransactionInfo { get; }

        public WithdrawalError Error { get; }

        public long? WithdrawalOperationId { get; private set; }

        public DateTime CreatedDateTime { get; }

        public DateTime UpdatedDateTime { get; private set; }


        public static Withdrawal Create(
            long id,
            long brokerAccountId,
            long brokerAccountRequisitesId,
            long? accountId,
            string referenceId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationRequisites destinationRequisites)
        {
            return new Withdrawal(
                id,
                0,
                0,
                brokerAccountId,
                brokerAccountRequisitesId,
                accountId,
                referenceId,
                unit,
                tenantId,
                fees,
                destinationRequisites,
                WithdrawalState.Processing,
                null,
                null,
                null,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public static Withdrawal Restore(
            long id,
            uint version,
            long sequence,
            long brokerAccountId,
            long brokerAccountRequisitesId,
            long? accountId,
            string referenceId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationRequisites destinationRequisites,
            WithdrawalState state,
            TransactionInfo transactionInfo,
            WithdrawalError error,
            long? withdrawalOperationId,
            DateTime createdDateTime,
            DateTime updatedDateTime)
        {
            return new Withdrawal(
                id,
                version,
                sequence,
                brokerAccountId,
                brokerAccountRequisitesId,
                accountId,
                referenceId,
                unit,
                tenantId,
                fees,
                destinationRequisites,
                state,
                transactionInfo,
                error,
                withdrawalOperationId,
                createdDateTime,
                updatedDateTime);
        }

        public void AddOperation(long operationId)
        {
            this.WithdrawalOperationId = operationId;
            this.UpdatedDateTime = DateTime.UtcNow;
            this.State = WithdrawalState.Executing;
        }
    }
}
