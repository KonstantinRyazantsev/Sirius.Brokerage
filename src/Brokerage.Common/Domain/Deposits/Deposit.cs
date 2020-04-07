using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swisschain.Sirius.Brokerage.MessagingContract;

namespace Brokerage.Common.Domain.Deposits
{
    public class Deposit
    {
        private Deposit(
            long id,
            uint version,
            long sequence,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            long assetId,
            decimal amount,
            IReadOnlyCollection<Fee> fees,
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
            Id = id;
            Version = version;
            Sequence = sequence;
            BrokerAccountRequisitesId = brokerAccountRequisitesId;
            AccountRequisitesId = accountRequisitesId;
            AssetId = assetId;
            Amount = amount;
            Fees = fees;
            TransactionInfo = transactionInfo;
            Error = error;
            DepositState = depositState;
            Sources = sources;
            DetectedDateTime = detectedDateTime;
            ConfirmedDateTime = confirmedDateTime;
            CompletedDateTime = completedDateTime;
            FailedDateTime = failedDateTime;
            CancelledDateTime = cancelledDateTime;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; private set; }
        public long BrokerAccountRequisitesId { get; }
        public long? AccountRequisitesId { get; }
        public long AssetId { get; }
        public decimal Amount { get; }
        public IReadOnlyCollection<Fee> Fees { get; }
        public TransactionInfo TransactionInfo { get; }
        public DepositError Error { get; }
        public DepositState DepositState { get; private set; }
        public IReadOnlyCollection<DepositSource> Sources { get; }
        public DateTime DetectedDateTime { get; }
        public DateTime? ConfirmedDateTime { get; private set; }
        public DateTime? CompletedDateTime { get; private set; }
        public DateTime? FailedDateTime { get; }
        public DateTime? CancelledDateTime { get; }

        public List<object> Events { get; } = new List<object>();

        public static Deposit Create(
            long id,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            long assetId,
            decimal amount,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<DepositSource> sources)
        {
            var deposit = new Deposit(
                id,
                default,
                0,
                brokerAccountRequisitesId,
                accountRequisitesId,
                assetId,
                amount,
                Array.Empty<Fee>(),
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
            long assetId,
            decimal amount,
            IReadOnlyCollection<Fee> fees,
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
                assetId,
                amount,
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
            this.Sequence++;
            Events.Add(new DepositUpdated()
            {
                DepositId = this.Id,
                Sequence = this.Sequence,
                AssetId = this.AssetId,
                BrokerAccountRequisitesId = this.BrokerAccountRequisitesId,
                Sources = this.Sources
                    .Select(x => new Swisschain.Sirius.Brokerage.MessagingContract.DepositSource()
                    {
                        Amount = x.Amount,
                        Address = x.Address
                    })
                    .ToArray(),
                AccountRequisitesId = this.AccountRequisitesId,
                Fees = this.Fees
                    .Select(x => new Swisschain.Sirius.Brokerage.MessagingContract.Fee()
                    {
                        Amount = x.Amount,
                        AssetId = x.AssetId
                    })
                    .ToArray(),
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
                        // TODO: Map here all possible tech issues
                        Code = Swisschain.Sirius.Brokerage.MessagingContract.DepositError.DepositErrorCode.TechnicalProblem,
                        Message = this.Error.Message
                    },
                ConfirmedDateTime = this.ConfirmedDateTime,
                DetectedDateTime = this.DetectedDateTime,
                CompletedDateTime = this.CompletedDateTime,
                FailedDateTime = this.FailedDateTime,
                CancelledDateTime = this.CancelledDateTime,
                Amount = this.Amount,
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
            var date = DateTime.UtcNow;

            if (!IsBrokerDeposit)
            {
                this.DepositState = DepositState.Confirmed;
                this.ConfirmedDateTime = date;
            }
            else
            {
                this.DepositState = DepositState.Completed;
                this.ConfirmedDateTime = date;
                this.CompletedDateTime = date;
            }

            this.AddDepositUpdatedEvent();
        }
    }
}
