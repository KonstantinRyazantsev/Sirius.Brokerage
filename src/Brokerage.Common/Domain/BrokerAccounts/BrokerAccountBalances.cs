using System;
using System.Collections.Generic;
using Swisschain.Sirius.Brokerage.MessagingContract;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountBalances
    {
        private BrokerAccountBalances(
            long id,
            long sequence,
            uint version,
            BrokerAccountBalancesId naturalId,
            decimal ownedBalance,
            decimal availableBalance,
            decimal pendingBalance,
            decimal reservedBalance,
            DateTime ownedBalanceUpdatedAt,
            DateTime availableBalanceUpdatedAt,
            DateTime pendingBalanceUpdatedAt,
            DateTime reservedBalanceUpdatedAt)
        {
            Id = id;
            Version = version;
            NaturalId = naturalId;
            OwnedBalance = ownedBalance;
            AvailableBalance = availableBalance;
            PendingBalance = pendingBalance;
            ReservedBalance = reservedBalance;
            OwnedBalanceUpdatedAt = ownedBalanceUpdatedAt;
            AvailableBalanceUpdatedAt = availableBalanceUpdatedAt;
            PendingBalanceUpdatedAt = pendingBalanceUpdatedAt;
            ReservedBalanceUpdatedAt = reservedBalanceUpdatedAt;
            Sequence = sequence;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; set; }
        public BrokerAccountBalancesId NaturalId { get; }
        public decimal OwnedBalance { get; private set; }
        public decimal AvailableBalance { get; private set; }
        public decimal PendingBalance { get; private set; }
        public decimal ReservedBalance { get; }
        public DateTime OwnedBalanceUpdatedAt { get; private set; }
        public DateTime AvailableBalanceUpdatedAt { get; private set; }
        public DateTime PendingBalanceUpdatedAt { get; private set; }
        public DateTime ReservedBalanceUpdatedAt { get; }

        public List<object> Events { get; } = new List<object>();

        public static BrokerAccountBalances Create(long id, BrokerAccountBalancesId naturalId)
        {
            return new BrokerAccountBalances(
                id,
                0,
                0,
                naturalId,
                pendingBalance: 0,
                ownedBalance: 0,
                availableBalance: 0,
                reservedBalance: 0,
                pendingBalanceUpdatedAt: DateTime.UtcNow,
                ownedBalanceUpdatedAt: DateTime.UtcNow,
                availableBalanceUpdatedAt: DateTime.UtcNow,
                reservedBalanceUpdatedAt: DateTime.UtcNow);
        }

        public static BrokerAccountBalances Restore(
            long id,
            long sequence,
            uint version,
            BrokerAccountBalancesId naturalId,
            decimal ownedBalance,
            decimal availableBalance,
            decimal pendingBalance,
            decimal reservedBalance,
            DateTime ownedBalanceUpdatedAt,
            DateTime availableBalanceUpdatedAt,
            DateTime pendingBalanceUpdatedAt,
            DateTime reservedBalanceUpdatedAt)
        {
            return new BrokerAccountBalances(
                id,
                sequence,
                version,
                naturalId,
                ownedBalance,
                availableBalance,
                pendingBalance,
                reservedBalance,
                ownedBalanceUpdatedAt,
                availableBalanceUpdatedAt,
                pendingBalanceUpdatedAt,
                reservedBalanceUpdatedAt);
        }

        public void AddPendingBalance(decimal amount)
        {
            PendingBalance += amount;
            PendingBalanceUpdatedAt = DateTime.UtcNow;

            GenerateEvent();
        }

        public void ConfirmRegularPendingBalance(decimal amount)
        {
            PendingBalance -= amount;
            OwnedBalance += amount;

            var updateDateTime = DateTime.UtcNow;

            PendingBalanceUpdatedAt = updateDateTime;
            OwnedBalanceUpdatedAt = updateDateTime;

            GenerateEvent();
        }

        public void ConfirmBrokerPendingBalance(decimal amount)
        {
            PendingBalance -= amount;
            OwnedBalance += amount;
            AvailableBalance += amount;

            var updateDateTime = DateTime.UtcNow;

            PendingBalanceUpdatedAt = updateDateTime;
            OwnedBalanceUpdatedAt = updateDateTime;
            AvailableBalanceUpdatedAt = updateDateTime;

            GenerateEvent();
        }

        public void ConsolidateBalance(decimal amount)
        {
            AvailableBalance += amount;

            AvailableBalanceUpdatedAt = DateTime.UtcNow;
            
            GenerateEvent();
        }

        private void GenerateEvent()
        {
            Events.Add(new BrokerAccountBalancesUpdated
            {
                BrokerAccountId = this.NaturalId.BrokerAccountId,
                AssetId = this.NaturalId.AssetId,
                Sequence = this.Sequence,
                ReservedBalanceUpdatedAt = this.ReservedBalanceUpdatedAt,
                AvailableBalance = this.AvailableBalance,
                OwnedBalance = this.OwnedBalance,
                OwnedBalanceUpdatedAt = this.OwnedBalanceUpdatedAt,
                AvailableBalanceUpdatedAt = this.AvailableBalanceUpdatedAt,
                ReservedBalance = this.ReservedBalance,
                PendingBalanceUpdatedAt = this.PendingBalanceUpdatedAt,
                PendingBalance = this.PendingBalance,
                BrokerAccountBalancesId = this.Id
            });

            Sequence++;
        }
    }
}
