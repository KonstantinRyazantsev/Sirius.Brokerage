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
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationDetails destinationDetails,
            WithdrawalState state,
            TransactionInfo transactionInfo,
            WithdrawalError error,
            long? operationId,
            DateTime createdAt,
            DateTime updatedAt,
            UserContext userContext)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            BrokerAccountDetailsId = brokerAccountDetailsId;
            AccountId = accountId;
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
            UserContext = userContext;
            Version = version;
            Sequence = sequence;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; private set; }
        public long BrokerAccountId { get; }
        public long BrokerAccountDetailsId { get; }
        public long? AccountId { get; }
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
        public UserContext UserContext { get; }
        public List<object> Events { get; } = new List<object>();
        
        public static Withdrawal Create(
            long id,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountId,
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationDetails destinationDetails,
            UserContext userContext)
        {
            var createdAt = DateTime.UtcNow;
            var withdrawal = new Withdrawal(
                id,
                0,
                0,
                brokerAccountId,
                brokerAccountDetailsId,
                accountId,
                unit,
                tenantId,
                fees,
                destinationDetails,
                WithdrawalState.Processing,
                null,
                null,
                null,
                createdAt,
                createdAt,
                userContext);

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
            Unit unit,
            string tenantId,
            IReadOnlyCollection<Unit> fees,
            DestinationDetails destinationDetails,
            WithdrawalState state,
            TransactionInfo transactionInfo,
            WithdrawalError error,
            long? operationId,
            DateTime createdAt,
            DateTime updatedDateTime,
            UserContext userContext)
        {
            return new Withdrawal(
                id,
                version,
                sequence,
                brokerAccountId,
                brokerAccountDetailsId,
                accountId,
                unit,
                tenantId,
                fees,
                destinationDetails,
                state,
                transactionInfo,
                error,
                operationId,
                createdAt,
                updatedDateTime,
                userContext);
        }

        public async Task<Operation> Execute(
            IBrokerAccountsRepository brokerAccountsRepository,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IOperationsFactory operationsFactory)
        {
            SwitchState(new[] {WithdrawalState.Processing}, WithdrawalState.Executing);

            var brokerAccountDetails = await brokerAccountDetailsRepository.Get(BrokerAccountDetailsId);
            var brokerAccount = await brokerAccountsRepository.Get(BrokerAccountId);

            var operation = await operationsFactory.StartWithdrawal(
                TenantId,
                Id,
                brokerAccountDetails.NaturalId.Address,
                DestinationDetails,
                Unit,
                brokerAccount.VaultId,
                UserContext);
            
            OperationId = operation.Id;
            UpdatedAt = DateTime.UtcNow;

            AddUpdateEvent();

            return operation;
        }

        public void TrackSent()
        {
            SwitchState(new[] {WithdrawalState.Executing}, WithdrawalState.Sent);
            
            UpdatedAt = DateTime.UtcNow;

            AddUpdateEvent();
        }

        public void Complete()
        {
            SwitchState(new[] {WithdrawalState.Sent}, WithdrawalState.Completed);
            
            UpdatedAt = DateTime.UtcNow;

            AddUpdateEvent();
        }

        public void Fail(WithdrawalError error)
        {
            SwitchState(new[] {WithdrawalState.Executing, WithdrawalState.Sent}, WithdrawalState.Failed);
            
            UpdatedAt = DateTime.UtcNow;
            Error = error;

            AddUpdateEvent();
        }

        private void SwitchState(IEnumerable<WithdrawalState> allowedStates, WithdrawalState targetState)
        {
            if (!allowedStates.Contains(State))
            {
                throw new InvalidOperationException($"Can't switch withdrawal to the {targetState} from the state {State}");
            }

            Sequence++;

            State = targetState;
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
                AccountId = AccountId,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                UserContext = new Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals.UserContext()
                {
                    AccountReferenceId = UserContext.AccountReferenceId,
                    ApiKeyId = UserContext.ApiKeyId,
                    WithdrawalReferenceId = UserContext.WithdrawalReferenceId,
                    UserId = UserContext.UserId,
                    PassClientIp = UserContext.PassClientIp
                }
            });
        }
    }
}
