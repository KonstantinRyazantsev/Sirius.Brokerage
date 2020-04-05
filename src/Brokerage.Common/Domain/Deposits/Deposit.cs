using System;
using System.Collections.Generic;
using System.Text;

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
            IReadOnlyCollection<DepositFee> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState depositState,
            IReadOnlyCollection<DepositSource> sources,
            string address,
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
            Address = address;
            DetectedDateTime = detectedDateTime;
            ConfirmedDateTime = confirmedDateTime;
            CompletedDateTime = completedDateTime;
            FailedDateTime = failedDateTime;
            CancelledDateTime = cancelledDateTime;
        }
        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; }
        public long BrokerAccountRequisitesId { get; }
        public long? AccountRequisitesId { get; }
        public long AssetId { get; }
        public decimal Amount { get; }
        public IReadOnlyCollection<DepositFee> Fees { get; }
        public TransactionInfo TransactionInfo { get; }
        public DepositError Error { get; }
        public DepositState DepositState { get; }
        public IReadOnlyCollection<DepositSource> Sources { get; }
        public string Address { get; }
        public DateTime DetectedDateTime { get; }
        public DateTime? ConfirmedDateTime { get; }
        public DateTime? CompletedDateTime { get; }
        public DateTime? FailedDateTime { get; }
        public DateTime? CancelledDateTime { get; }

        public List<object> Events { get; } = new List<object>();

        public static Deposit Create(
            long id,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            long assetId,
            decimal amount,
            IReadOnlyCollection<DepositFee> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            IReadOnlyCollection<DepositSource> sources,
            string address)
        {
            return new Deposit(
                id,
                default,
                0,
                brokerAccountRequisitesId,
                accountRequisitesId,
                assetId,
                amount,
                fees,
                transactionInfo,
                error,
                DepositState.Detected,
                sources,
                address,
                DateTime.UtcNow,
                null,
                null,
                null,
                null);
        }

        public static Deposit Restore(
            long id,
            uint version,
            long sequence,
            long brokerAccountRequisitesId,
            long? accountRequisitesId,
            long assetId,
            decimal amount,
            IReadOnlyCollection<DepositFee> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState depositState,
            IReadOnlyCollection<DepositSource> sources,
            string address,
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
                address,
                detectedDateTime,
                confirmedDateTime,
                completedDateTime,
                failedDateTime,
                cancelledDateTime);
        }
    }

    public class DepositFee
    {
        public DepositFee(long assetId, decimal amount)
        {
            AssetId = assetId;
            Amount = amount;
        }

        public long AssetId { get; }

        public decimal Amount { get; }
    }

    public class DepositSource
    {
        public DepositSource(string address, decimal amount)
        {
            Address = address;
            Amount = amount;
        }
        public string Address { get; set; }

        public decimal Amount { get; set; }
    }

    public class TransactionInfo
    {
        public TransactionInfo(string transactionId, long transactionBlock, long requiredConfirmationsCount,
            DateTime dateTime)
        {
            TransactionId = transactionId;
            TransactionBlock = transactionBlock;
            RequiredConfirmationsCount = requiredConfirmationsCount;
            DateTime = dateTime;
        }

        public string TransactionId { get; }
        public long TransactionBlock { get; }
        public long RequiredConfirmationsCount { get; }
        public DateTime DateTime { get; }
    }

    public enum DepositState
    {
        Detected,
        Confirmed,
        Completed,
        Failed,
        Cancelled
    }

    public class DepositError
    {
        public DepositError(string message, DepositErrorCode code)
        {
            Message = message;
            Code = code;
        }
        public string Message { get; }

        public DepositErrorCode Code { get; }

        public enum DepositErrorCode
        {
            TechnicalProblem
        }
    }
}
