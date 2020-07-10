using System;
using System.Collections.Generic;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;

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
            DateTime createdAt,
            DateTime updatedAt)
        {
            Id = id;
            Version = version;
            NaturalId = naturalId;
            OwnedBalance = ownedBalance;
            AvailableBalance = availableBalance;
            PendingBalance = pendingBalance;
            ReservedBalance = reservedBalance;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Sequence = sequence;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; set; }
        public BrokerAccountBalancesId NaturalId { get; }
        public decimal OwnedBalance { get; private set; }
        public decimal AvailableBalance { get; private set; }
        public decimal PendingBalance { get; private set; }
        public decimal ReservedBalance { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }

        public List<object> Events { get; } = new List<object>();

        public static BrokerAccountBalances Create(long id, BrokerAccountBalancesId naturalId)
        {
            var createdAt = DateTime.UtcNow;
            return new BrokerAccountBalances(
                id,
                0,
                0,
                naturalId,
                pendingBalance: 0,
                ownedBalance: 0,
                availableBalance: 0,
                reservedBalance: 0,
                createdAt: createdAt,
                updatedAt: createdAt);
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
            DateTime createdAt,
            DateTime updatedAt)
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
                createdAt,
                updatedAt);
        }

        public void AddPendingBalance(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PendingBalance += amount;
            UpdatedAt = DateTime.UtcNow;

            GenerateEvent();
        }

        public void ConfirmRegularPendingBalance(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PendingBalance -= amount;
            OwnedBalance += amount;

            UpdatedAt = DateTime.UtcNow;

            GenerateEvent();
        }

        public void ConfirmBrokerPendingBalance(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PendingBalance -= amount;
            OwnedBalance += amount;
            AvailableBalance += amount;

            var updateDateTime = DateTime.UtcNow;

            UpdatedAt = updateDateTime;

            GenerateEvent();
        }

        public void ConfirmBrokerWithDestinationTagPendingBalance(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PendingBalance -= amount;
            OwnedBalance += amount;
            AvailableBalance += amount;

            var updateDateTime = DateTime.UtcNow;

            UpdatedAt = updateDateTime;

            GenerateEvent();
        }

        public void ConsolidateBalance(decimal receivedAmount, decimal fee)
        {
            if (receivedAmount <= 0 || fee <= 0)
            {
                return;
            }

            AvailableBalance += receivedAmount;
            OwnedBalance -= fee;

            UpdatedAt = DateTime.UtcNow;
            
            GenerateEvent();
        }

        public void ReserveBalance(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            AvailableBalance -= amount;
            ReservedBalance += amount;

            var dateTime = DateTime.UtcNow;

            UpdatedAt = dateTime;

            GenerateEvent();
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            ReservedBalance -= amount;
            OwnedBalance -= amount;

            var dateTime = DateTime.UtcNow;

            UpdatedAt = dateTime;

            GenerateEvent();
        }

        private void GenerateEvent()
        {
            Events.Add(new BrokerAccountBalancesUpdated
            {
                BrokerAccountId = this.NaturalId.BrokerAccountId,
                AssetId = this.NaturalId.AssetId,
                Sequence = this.Sequence,
                AvailableBalance = this.AvailableBalance,
                OwnedBalance = this.OwnedBalance,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
                ReservedBalance = this.ReservedBalance,
                PendingBalance = this.PendingBalance,
                BrokerAccountBalancesId = this.Id
            });

            Sequence++;
        }
    }
}
