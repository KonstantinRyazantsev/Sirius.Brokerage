﻿using System;
using System.Collections.Generic;
using System.Linq;
using Swisschain.Sirius.Brokerage.MessagingContract.Deposits;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;

namespace Brokerage.Common.Domain.Deposits
{
    // TODO: Natural ID
    public abstract class Deposit
    {
        protected Deposit(
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
            decimal minDepositForConsolidation,
            DepositType depositType)
        {
            Id = id;
            Version = version;
            Sequence = sequence;
            TenantId = tenantId;
            BlockchainId = blockchainId;
            BrokerAccountId = brokerAccountId;
            BrokerAccountDetailsId = brokerAccountDetailsId;
            AccountDetailsId = accountDetailsId;
            Unit = unit;
            Fees = fees;
            TransactionInfo = transactionInfo;
            Error = error;
            State = state;
            Sources = sources;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            ConsolidationOperationId = consolidationOperationId;
            MinDepositForConsolidation = minDepositForConsolidation;
            DepositType = depositType;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; protected set; }
        public string TenantId { get; }
        public string BlockchainId { get; }
        public long BrokerAccountId { get; }
        public long BrokerAccountDetailsId { get; }
        public long? AccountDetailsId { get; }
        public Unit Unit { get; }
        public IReadOnlyCollection<Unit> Fees { get; protected set; }
        public TransactionInfo TransactionInfo { get; protected set; }
        public DepositError Error { get; protected set; }
        public DepositState State { get; protected set; }
        public IReadOnlyCollection<DepositSource> Sources { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; protected set; }
        public long? ConsolidationOperationId { get; protected set; }
        public List<object> Events { get; } = new List<object>();
        public decimal MinDepositForConsolidation { get; }
        public DepositType DepositType { get; }

        protected void SwitchState(IEnumerable<DepositState> allowedStates, DepositState targetState)
        {
            if (!allowedStates.Contains(State))
            {
                throw new InvalidOperationException($"Can't switch deposit to the {targetState} from the state {State}");
            }

            Sequence++;

            State = targetState;
        }

        protected void AddDepositUpdatedEvent()
        {
            Events.Add(new DepositUpdated
            {
                DepositId = Id,
                Sequence = Sequence,
                TenantId = TenantId,
                BlockchainId = BlockchainId,
                BrokerAccountId = BrokerAccountId,
                Unit = Unit,
                BrokerAccountDetailsId = BrokerAccountDetailsId,
                Sources = Sources
                    .Select(x => new Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositSource()
                    {
                        Amount = x.Amount,
                        Address = x.Address
                    })
                    .ToArray(),
                AccountDetailsId = AccountDetailsId,
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
                    : new Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositError()
                    {
                        Code = Error.Code switch
                        {
                            DepositErrorCode.TechnicalProblem =>
                            Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositError.DepositErrorCode.TechnicalProblem,
                            DepositErrorCode.ValidationRejected =>
                            Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositError.DepositErrorCode.ValidationRejected,
                            DepositErrorCode.SigningRejected => 
                            Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositError.DepositErrorCode.SigningRejected,

                            _ => throw new ArgumentOutOfRangeException(nameof(Error.Code), Error.Code, null)
                        },
                        Message = Error.Message
                    },
                UpdatedAt = UpdatedAt,
                CreatedAt = CreatedAt,
                State = State switch
                {
                    DepositState.Detected => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositState.Detected,
                    DepositState.Confirmed => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositState.Confirmed,
                    DepositState.Completed => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositState.Completed,
                    DepositState.Failed => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositState.Failed,
                    DepositState.Cancelled => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositState.Cancelled,
                    
                    _ => throw new ArgumentOutOfRangeException(nameof(State), State, null)
                },
                DepositType = DepositType switch {
                    DepositType.Tiny => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositType.Tiny,
                    DepositType.Broker => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositType.Broker,
                    DepositType.Regular => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositType.Regular,
                    DepositType.Token => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositType.Token,
                    DepositType.TinyToken => Swisschain.Sirius.Brokerage.MessagingContract.Deposits.DepositType.TinyToken,

                    _ => throw new ArgumentOutOfRangeException(nameof(DepositType), DepositType, null)
                }
            });
        }
    }
}
