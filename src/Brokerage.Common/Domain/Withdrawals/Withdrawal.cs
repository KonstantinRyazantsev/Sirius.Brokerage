using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Persistence.BrokerAccounts;
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
            long brokerAccountDetailsId,
            long? accountId,
            string referenceId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationDetails destinationDetails,
            WithdrawalState state,
            TransactionInfo transactionInfo,
            WithdrawalError error,
            long? operationId,
            DateTime createdAt,
            DateTime updatedAt)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            BrokerAccountDetailsId = brokerAccountDetailsId;
            AccountId = accountId;
            ReferenceId = referenceId;
            Unit = unit;
            TenantId = tenantId;
            Fees = fees;
            DestinationDetails = destinationDetails;
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

        public long BrokerAccountDetailsId { get; }

        public long? AccountId { get; }

        public string ReferenceId { get; }

        public Unit Unit { get; }

        public string TenantId { get; }

        public IReadOnlyCollection<Unit> Fees { get; }

        public DestinationDetails DestinationDetails { get; }

        public WithdrawalState State { get; private set; }

        public TransactionInfo TransactionInfo { get; }

        public WithdrawalError Error { get; private set; }

        public long? OperationId { get; private set; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public List<object> Events { get; } = new List<object>();

        public static Withdrawal Create(
            long id,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountId,
            string referenceId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationDetails destinationDetails)
        {
            var createdAt = DateTime.UtcNow;
            var withdrawal = new Withdrawal(
                id,
                0,
                0,
                brokerAccountId,
                brokerAccountDetailsId,
                accountId,
                referenceId,
                unit,
                tenantId,
                fees,
                destinationDetails,
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
            long brokerAccountDetailsId,
            long? accountId,
            string referenceId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationDetails destinationDetails,
            WithdrawalState state,
            TransactionInfo transactionInfo,
            WithdrawalError error,
            long? operationId,
            DateTime createdAt,
            DateTime updatedDateTime)
        {
            return new Withdrawal(
                id,
                version,
                sequence,
                brokerAccountId,
                brokerAccountDetailsId,
                accountId,
                referenceId,
                unit,
                tenantId,
                fees,
                destinationDetails,
                state,
                transactionInfo,
                error,
                operationId,
                createdAt,
                updatedDateTime);
        }

        public async Task Execute(
            IBrokerAccountsRepository brokerAccountsRepository,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IOperationsExecutor operationsExecutor)
        {
            if (SwitchState(new[] {WithdrawalState.Processing}, WithdrawalState.Executing))
            {
                var brokerAccountDetails = await brokerAccountDetailsRepository.Get(BrokerAccountDetailsId);
                var brokerAccount = await brokerAccountsRepository.Get(BrokerAccountId);

                var operation = await operationsExecutor.StartWithdrawal(
                    TenantId,
                    Id,
                    brokerAccountDetails.NaturalId.Address,
                    DestinationDetails,
                    Unit,
                    brokerAccount.VaultId);

                OperationId = operation.Id;
                UpdatedAt = DateTime.UtcNow;
            }

            AddUpdateEvent();
        }

        public void TrackSent()
        {
            if (SwitchState(new[] {WithdrawalState.Executing}, WithdrawalState.Sent))
            {
                UpdatedAt = DateTime.UtcNow;
            }

            AddUpdateEvent();
        }

        public void Complete()
        {
            if (SwitchState(new[] {WithdrawalState.Sent}, WithdrawalState.Completed))
            {
                UpdatedAt = DateTime.UtcNow;
            }

            AddUpdateEvent();
        }

        public void Fail(WithdrawalError error)
        {
            if (SwitchState(new[] {WithdrawalState.Executing, WithdrawalState.Sent}, WithdrawalState.Failed))
            {
                UpdatedAt = DateTime.UtcNow;
                Error = error;
            }

            AddUpdateEvent();
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

            Sequence++;

            State = targetState;

            return true;
        }

        private void AddUpdateEvent()
        {
            Events.Add(new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalUpdated()
            {
                WithdrawalId = Id,
                TransactionInfo =
                    TransactionInfo == null
                        ? null
                        : new Swisschain.Sirius.Brokerage.MessagingContract.TransactionInfo()
                        {
                            TransactionId = TransactionInfo.TransactionId,
                            TransactionBlock = TransactionInfo.TransactionBlock,
                            DateTime = TransactionInfo.DateTime,
                            RequiredConfirmationsCount = TransactionInfo.RequiredConfirmationsCount
                        },
                Sequence = Sequence,
                TenantId = TenantId,
                BrokerAccountId = BrokerAccountId,
                BrokerAccountDetailsId = BrokerAccountDetailsId,
                Fees = Fees,
                Unit = Unit,
                DestinationDetails = new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.DestinationDetails()
                {
                    TagType = DestinationDetails.TagType,
                    Tag = DestinationDetails.Tag,
                    Address = DestinationDetails.Address
                },
                Error = Error == null
                    ? null
                    : new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalError()
                    {
                        Code = Error.Code switch
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
                            _ => throw new ArgumentOutOfRangeException(nameof(Error.Code),
                                Error.Code,
                                null)
                        },
                        Message = Error.Message
                    },
                State = State switch
                {
                    WithdrawalState.Processing => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState
                        .Processing,
                    WithdrawalState.Executing => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState
                        .Executing,
                    WithdrawalState.Sent => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState.Sent,
                    WithdrawalState.Completed => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState
                        .Completed,
                    WithdrawalState.Failed => Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.WithdrawalState.Failed,
                    _ => throw new ArgumentOutOfRangeException(nameof(State), State, null)
                },
                OperationId = OperationId,
                ReferenceId = ReferenceId,
                AccountId = AccountId,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            });
        }
    }
}
