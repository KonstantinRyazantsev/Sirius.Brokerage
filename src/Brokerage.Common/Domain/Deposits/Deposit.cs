using System;
using System.Collections.Generic;
using System.Linq;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits
{
    // TODO: Natural ID
    public class Deposit
    {
        private Deposit(
            long id,
            uint version,
            long sequence,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            Unit unit,
            long? consolidationOperationId,
            IReadOnlyCollection<Unit> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState depositState,
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
            BrokerAccountRequisitesId = brokerAccountRequisitesId;
            AccountRequisitesId = accountRequisitesId;
            Unit = unit;
            Fees = fees;
            TransactionInfo = transactionInfo;
            Error = error;
            DepositState = depositState;
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
        public long BrokerAccountRequisitesId { get; }
        public long? AccountRequisitesId { get; }
        public Unit Unit { get; }
        public IReadOnlyCollection<Unit> Fees { get; }
        public TransactionInfo TransactionInfo { get; }
        public DepositError Error { get; private set; }
        public DepositState DepositState { get; private set; }
        public IReadOnlyCollection<DepositSource> Sources { get; }
        public DateTime DetectedAt { get; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public DateTime? CancelledAt { get; }

        public long? ConsolidationOperationId { get; private set; }
        public List<object> Events { get; } = new List<object>();

        public static Deposit Create(
            long id,
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

        private void AddDepositUpdatedEvent()
        {
            Events.Add(new DepositUpdated
            {
                DepositId = this.Id,
                Sequence = this.Sequence,
                Unit = this.Unit,
                BrokerAccountRequisitesId = this.BrokerAccountRequisitesId,
                Sources = this.Sources
                    .Select(x => new Swisschain.Sirius.Brokerage.MessagingContract.DepositSource()
                    {
                        Amount = x.Amount,
                        Address = x.Address
                    })
                    .ToArray(),
                AccountRequisitesId = this.AccountRequisitesId,
                Fees = this.Fees,
                TransactionInfo = new Swisschain.Sirius.Brokerage.MessagingContract.TransactionInfo()
                {
                    TransactionId = this.TransactionInfo.TransactionId,
                    TransactionBlock = this.TransactionInfo.TransactionBlock,
                    DateTime = this.TransactionInfo.DateTime,
                    RequiredConfirmationsCount = this.TransactionInfo.RequiredConfirmationsCount
                },
                Error = this.Error == null
                    ? null
                    : new Swisschain.Sirius.Brokerage.MessagingContract.DepositError()
                    {
                        Code = Swisschain.Sirius.Brokerage.MessagingContract.DepositError.DepositErrorCode.TechnicalProblem,
                        Message = this.Error.Message
                    },
                ConfirmedAt = this.ConfirmedAt,
                DetectedAt = this.DetectedAt,
                CompletedAt = this.CompletedAt,
                FailedAt = this.FailedAt,
                CancelledAt = this.CancelledAt,
                State = this.DepositState switch
                {
                    DepositState.Detected => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Detected,
                    DepositState.Confirmed => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Confirmed,
                    DepositState.Completed => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Completed,
                    DepositState.Failed => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Failed,
                    DepositState.Cancelled => Swisschain.Sirius.Brokerage.MessagingContract.DepositState.Cancelled,
                    _ => throw new ArgumentOutOfRangeException(nameof(this.DepositState), this.DepositState, null)
                }
            });
        }

        public bool IsBrokerDeposit => this.AccountRequisitesId == null;

        public void Confirm()
        {
            if (this.DepositState != DepositState.Detected && 
                this.DepositState != DepositState.Confirmed)
                throw new InvalidOperationException($"Can't confirm deposit with state {this.DepositState}");

            if (this.DepositState != DepositState.Confirmed)
            {
                this.Sequence++;

                var date = DateTime.UtcNow;

                if (!IsBrokerDeposit)
                {
                    this.DepositState = DepositState.Confirmed;
                    this.ConfirmedAt = date;
                }
                else
                {
                    this.DepositState = DepositState.Completed;
                    this.ConfirmedAt = date;
                    this.CompletedAt = date;
                }
            }

            this.AddDepositUpdatedEvent();
        }

        public void TrackConsolidationOperation(long operationId)
        {
            this.ConsolidationOperationId = operationId;
        }

        public void Complete()
        {
            if (this.DepositState != DepositState.Confirmed &&
                this.DepositState != DepositState.Completed)
                throw new InvalidOperationException($"Can't complete deposit with state {this.DepositState}");

            if (this.DepositState != DepositState.Completed)
            {
                this.Sequence++;

                this.DepositState = DepositState.Completed;
                this.CompletedAt = DateTime.UtcNow;
            }

            this.AddDepositUpdatedEvent();
        }

        public void Fail(DepositError depositError)
        {
            if (this.DepositState != DepositState.Confirmed &&
                this.DepositState != DepositState.Failed)
                throw new InvalidOperationException($"Can't fail deposit with state {this.DepositState}");

            if (this.DepositState != DepositState.Failed)
            {
                this.Sequence++;

                this.DepositState = DepositState.Failed;
                this.FailedAt = DateTime.UtcNow;
                this.Error = depositError;
            }

            this.AddDepositUpdatedEvent();
        }
    }
}
