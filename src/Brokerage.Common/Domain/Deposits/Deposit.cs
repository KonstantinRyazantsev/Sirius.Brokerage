using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Executor.ApiContract.Common;
using Swisschain.Sirius.Executor.ApiContract.Transfers;
using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;

namespace Brokerage.Common.Domain.Deposits
{
    // TODO: Natural ID
    public class Deposit
    {
        private Deposit(
            long id,
            uint version,
            long sequence,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            Unit unit,
            long? consolidationOperationId,
            IReadOnlyCollection<Unit> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState state,
            IReadOnlyCollection<DepositSource> sources,
            DateTime detectedAt,
            DateTime? confirmedAt,
            DateTime? completedAt,
            DateTime? failedAt,
            DateTime? cancelledAt)
        {
            Id = id;
            Version = version;
            Sequence = sequence;
            TenantId = tenantId;
            BlockchainId = blockchainId;
            BrokerAccountId = brokerAccountId;
            BrokerAccountRequisitesId = brokerAccountRequisitesId;
            AccountRequisitesId = accountRequisitesId;
            Unit = unit;
            Fees = fees;
            TransactionInfo = transactionInfo;
            Error = error;
            State = state;
            Sources = sources;
            DetectedAt = detectedAt;
            ConfirmedAt = confirmedAt;
            CompletedAt = completedAt;
            FailedAt = failedAt;
            CancelledAt = cancelledAt;
            ConsolidationOperationId = consolidationOperationId;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; private set; }
        public string TenantId { get; }
        public string BlockchainId { get; }
        public long BrokerAccountId { get; }
        public long BrokerAccountRequisitesId { get; }
        public long? AccountRequisitesId { get; }
        public Unit Unit { get; }
        public IReadOnlyCollection<Unit> Fees { get; }
        public TransactionInfo TransactionInfo { get; }
        public DepositError Error { get; private set; }
        public DepositState State { get; private set; }
        public IReadOnlyCollection<DepositSource> Sources { get; }
        public DateTime DetectedAt { get; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public DateTime? CancelledAt { get; }
        public long? ConsolidationOperationId { get; private set; }

        public List<object> Events { get; } = new List<object>();

        public bool IsBrokerDeposit => AccountRequisitesId == null;
        
        public static Deposit Create(
            long id,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            Unit unit,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<DepositSource> sources)
        {
            var deposit = new Deposit(
                id,
                default,
                0,
                tenantId,
                blockchainId,
                brokerAccountId,
                brokerAccountRequisitesId,
                accountRequisitesId,
                unit,
                null,
                Array.Empty<Unit>(),
                transactionInfo,
                null,
                DepositState.Detected,
                sources,
                DateTime.UtcNow,
                null,
                null,
                null,
                null);

            deposit.AddDepositUpdatedEvent();

            return deposit;
        }

        public static Deposit Restore(
            long id,
            uint version,
            long sequence,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            Unit unit,
            long? consolidationOperationId,
            IReadOnlyCollection<Unit> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState depositState,
            IReadOnlyCollection<DepositSource> sources,
            DateTime detectedDateTime,
            DateTime? confirmedDateTime,
            DateTime? completedDateTime,
            DateTime? failedDateTime,
            DateTime? cancelledDateTime)
        {
            return new Deposit(
                id,
                version,
                sequence,
                tenantId,
                blockchainId,
                brokerAccountId,
                brokerAccountRequisitesId,
                accountRequisitesId,
                unit,
                consolidationOperationId,
                fees,
                transactionInfo,
                error,
                depositState,
                sources,
                detectedDateTime,
                confirmedDateTime,
                completedDateTime,
                failedDateTime,
                cancelledDateTime);
        }

        public async Task ConfirmRegular(IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository, 
            TransactionConfirmed tx, 
            IExecutorClient executorClient)
        {
            if (IsBrokerDeposit)
            {
                throw new InvalidOperationException("Can't confirm a broker deposit as a regular deposit");
            }

            if (SwitchState(new[] {DepositState.Detected}, DepositState.Confirmed))
            {
                // TODO: Optimize this - track in the processingContext, to avoid multiple reading
                var brokerAccountRequisites = await brokerAccountRequisitesRepository.GetAsync(BrokerAccountRequisitesId);
                // ReSharper disable once PossibleInvalidOperationException
                var accountRequisites = await accountRequisitesRepository.GetAsync(AccountRequisitesId.Value);

                var response = await executorClient.Transfers.ExecuteAsync(
                    new ExecuteTransferRequest(new ExecuteTransferRequest
                    {
                        AssetId = Unit.AssetId,
                        Operation = new OperationRequest
                        {
                            AsAtBlockNumber = tx.BlockNumber + tx.RequiredConfirmationsCount,
                            RequestId = $"Brokerage:DepositConsolidation:{Id}",
                            FeePayerAddress = brokerAccountRequisites.NaturalId.Address,
                            TenantId = TenantId
                        },
                        Movements =
                        {
                            new Movement
                            {
                                SourceAddress = accountRequisites.NaturalId.Address,
                                DestinationAddress = brokerAccountRequisites.NaturalId.Address,
                                Amount = Unit.Amount,
                            }
                        }
                    }));

                if (response.BodyCase == ExecuteTransferResponse.BodyOneofCase.Error)
                {
                    throw new InvalidOperationException($"{response.Error.ErrorCode} {response.Error.ErrorMessage}");
                }

                ConsolidationOperationId = response.Response.Operation.Id;
                ConfirmedAt = DateTime.UtcNow;
            }

            AddDepositUpdatedEvent();
        }

        public void ConfirmBroker()
        {
            if (!IsBrokerDeposit)
            {
                throw new InvalidOperationException("Can't confirm a regular deposit as a broker deposit");
            }

            if (SwitchState(new[] {DepositState.Detected}, DepositState.Completed))
            {
                var date = DateTime.UtcNow;

                ConfirmedAt = date;
                CompletedAt = date;
            }

            AddDepositUpdatedEvent();
        }

        public void Complete()
        {
            if (SwitchState(new[] {DepositState.Confirmed}, DepositState.Completed))
            {
                CompletedAt = DateTime.UtcNow;
            }

            AddDepositUpdatedEvent();
        }

        public void Fail(DepositError depositError)
        {
            if (SwitchState(new[] {DepositState.Confirmed}, DepositState.Failed))
            {
                FailedAt = DateTime.UtcNow;
                Error = depositError;
            }

            AddDepositUpdatedEvent();
        }

        private bool SwitchState(IEnumerable<DepositState> allowedStates, DepositState targetState)
        {
            if (State == targetState)
            {
                return false;
            }

            if (!allowedStates.Contains(State))
            {
                throw new InvalidOperationException($"Can't switch deposit to the {targetState} from the state {State}");
            }

            Sequence++;

            State = targetState;

            return true;
        }

        private void AddDepositUpdatedEvent()
        {
            Events.Add(new DepositUpdated
            {
                DepositId = Id,
                Sequence = Sequence,
                TenantId = TenantId,
                BlockchainId = BlockchainId,
                BrokerAccountId = BrokerAccountId,
                Unit = Unit,
                BrokerAccountRequisitesId = BrokerAccountRequisitesId,
                Sources = Sources
                    .Select(x => new Swisschain.Sirius.Brokerage.MessagingContract.DepositSource()
                    {
                        Amount = x.Amount,
                        Address = x.Address
                    })
                    .ToArray(),
                AccountRequisitesId = AccountRequisitesId,
                Fees = Fees,
                TransactionInfo = new Swisschain.Sirius.Brokerage.MessagingContract.TransactionInfo()
                {
                    TransactionId = TransactionInfo.TransactionId,
                    TransactionBlock = TransactionInfo.TransactionBlock,
                    DateTime = TransactionInfo.DateTime,
                    RequiredConfirmationsCount = TransactionInfo.RequiredConfirmationsCount
                },
                Error = Error == null
                    ? null
                    : new Swisschain.Sirius.Brokerage.MessagingContract.DepositError()
                    {
                        Code = Swisschain.Sirius.Brokerage.MessagingContract.DepositError.DepositErrorCode.TechnicalProblem,
                        Message = Error.Message
                    },
                ConfirmedAt = ConfirmedAt,
                DetectedAt = DetectedAt,
                CompletedAt = CompletedAt,
                FailedAt = FailedAt,
                CancelledAt = CancelledAt,
                State = State switch
                {
                    DepositState.Detected => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Detected,
                    DepositState.Confirmed => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Confirmed,
                    DepositState.Completed => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Completed,
                    DepositState.Failed => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Failed,
                    DepositState.Cancelled => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Cancelled,
                    _ => throw new ArgumentOutOfRangeException(nameof(State), State, null)
                }
            });
        }
    }
}
