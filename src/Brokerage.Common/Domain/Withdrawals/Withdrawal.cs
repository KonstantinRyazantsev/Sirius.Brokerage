using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals;
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
            long? operationId,
            DateTime createdAt,
            DateTime updatedAt)
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
            OperationId = operationId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Version = version;
            Sequence = sequence;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; set; }
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

        public long? OperationId { get; private set; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public List<object> Events { get; } = new List<object>();

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
            var createdAt = DateTime.UtcNow;
            var withdrawal = new Withdrawal(
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
                createdAt,
                createdAt);

            withdrawal.AddUpdateEvent();

            return withdrawal;
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
            if (SwitchState(new[] {WithdrawalState.Processing}, WithdrawalState.Executing))
            {
                this.OperationId = operationId;
                this.UpdatedAt = DateTime.UtcNow;
            }

            this.AddUpdateEvent();
        }

        private bool SwitchState(IEnumerable<WithdrawalState> allowedStates, WithdrawalState targetState)
        {
            if (State == targetState)
            {
                return false;
            }

            if (!allowedStates.Contains(State))
            {
                throw new InvalidOperationException($"Can't switch withdrawal to the {targetState} from the state {State}");
            }

            this.Sequence++;

            State = targetState;

            return true;
        }

        private void AddUpdateEvent()
        {
            this.Events.Add(new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalUpdated()
            {
                WithdrawalId = this.Id,
                TransactionInfo =
                    this.TransactionInfo == null
                        ? null
                        : new Swisschain.Sirius.Brokerage.MessagingContract.TransactionInfo()
                        {
                            TransactionId = this.TransactionInfo.TransactionId,
                            TransactionBlock = this.TransactionInfo.TransactionBlock,
                            DateTime = this.TransactionInfo.DateTime,
                            RequiredConfirmationsCount = this.TransactionInfo.RequiredConfirmationsCount
                        },
                Sequence = this.Sequence,
                TenantId = this.TenantId,
                BrokerAccountId = this.BrokerAccountId,
                BrokerAccountRequisitesId = this.BrokerAccountRequisitesId,
                Fees = this.Fees,
                Unit = this.Unit,
                DestinationRequisites = new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.DestinationRequisites()
                {
                    TagType = this.DestinationRequisites.TagType,
                    Tag = this.DestinationRequisites.Tag,
                    Address = this.DestinationRequisites.Address
                },
                Error = this.Error == null
                    ? null
                    : new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalError()
                    {
                        Code = this.Error.Code switch
                        {
                            WithdrawalErrorCode.NotEnoughBalance =>
                            Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalErrorCode.NotEnoughBalance,
                            WithdrawalErrorCode.InvalidDestinationAddress =>
                            Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalErrorCode
                                .InvalidDestinationAddress,
                            WithdrawalErrorCode.DestinationTagRequired =>
                            Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalErrorCode
                                .DestinationTagRequired,
                            WithdrawalErrorCode.TechnicalProblem =>
                            Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalErrorCode.TechnicalProblem,
                            _ => throw new ArgumentOutOfRangeException(nameof(this.Error.Code),
                                this.Error.Code,
                                null)
                        },
                        Message = this.Error.Message
                    },
                State = this.State switch
                {
                    WithdrawalState.Processing => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState
                        .Processing,
                    WithdrawalState.Executing => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState
                        .Executing,
                    WithdrawalState.Sent => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState.Sent,
                    WithdrawalState.Completed => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState
                        .Completed,
                    WithdrawalState.Failed => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState.Failed,
                    _ => throw new ArgumentOutOfRangeException(nameof(this.State), this.State, null)
                },
                OperationId = this.OperationId,
                ReferenceId = this.ReferenceId,
                AccountId = this.AccountId,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            });
        }
    }
}
