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
            long brokerAccountId,
            long assetId,
            decimal ownedBalance,
            decimal availableBalance,
            decimal pendingBalance,
            decimal reservedBalance,
            DateTime ownedBalanceUpdateDateTime,
            DateTime availableBalanceUpdateDateTime,
            DateTime pendingBalanceUpdateDateTime,
            DateTime reservedBalanceUpdateDateTime)
        {
            Id = id;
            Version = version;
            BrokerAccountId = brokerAccountId;
            AssetId = assetId;
            OwnedBalance = ownedBalance;
            AvailableBalance = availableBalance;
            PendingBalance = pendingBalance;
            ReservedBalance = reservedBalance;
            OwnedBalanceUpdateDateTime = ownedBalanceUpdateDateTime;
            AvailableBalanceUpdateDateTime = availableBalanceUpdateDateTime;
            PendingBalanceUpdateDateTime = pendingBalanceUpdateDateTime;
            ReservedBalanceUpdateDateTime = reservedBalanceUpdateDateTime;
            Sequence = sequence;
        }

        public long Id { get; }
        public uint Version { get; }
        public long Sequence { get; set; }
        public long BrokerAccountId { get; }
        public long AssetId { get; }
        public decimal OwnedBalance { get; private set; }
        public decimal AvailableBalance { get; private set; }
        public decimal PendingBalance { get; private set; }
        public decimal ReservedBalance { get; }
        public DateTime OwnedBalanceUpdateDateTime { get; private set; }
        public DateTime AvailableBalanceUpdateDateTime { get; private set; }
        public DateTime PendingBalanceUpdateDateTime { get; private set; }
        public DateTime ReservedBalanceUpdateDateTime { get; }

        public List<object> Events { get; private set; } = new List<object>();

        public static BrokerAccountBalances Create(
            long id,
            long brokerAccountId,
            long assetId)
        {
            return new BrokerAccountBalances(
                id,
                0,
                0,
                brokerAccountId,
                assetId,
                pendingBalance: 0,
                ownedBalance: 0,
                availableBalance: 0,
                reservedBalance: 0,
                pendingBalanceUpdateDateTime: DateTime.UtcNow,
                ownedBalanceUpdateDateTime: DateTime.UtcNow,
                availableBalanceUpdateDateTime: DateTime.UtcNow,
                reservedBalanceUpdateDateTime: DateTime.UtcNow);
        }

        public static BrokerAccountBalances Restore(
            long id,
            long sequence,
            uint version,
            long brokerAccountId,
            long assetId,
            decimal ownedBalance,
            decimal availableBalance,
            decimal pendingBalance,
            decimal reservedBalance,
            DateTime ownedBalanceUpdateDateTime,
            DateTime availableBalanceUpdateDateTime,
            DateTime pendingBalanceUpdateDateTime,
            DateTime reservedBalanceUpdateDateTime)
        {
            return new BrokerAccountBalances(
                id,
                sequence,
                version,
                brokerAccountId,
                assetId,
                ownedBalance,
                availableBalance,
                pendingBalance,
                reservedBalance,
                ownedBalanceUpdateDateTime,
                availableBalanceUpdateDateTime,
                pendingBalanceUpdateDateTime,
                reservedBalanceUpdateDateTime);
        }

        public void AddPendingBalance(decimal amount)
        {
            PendingBalance += amount;
            PendingBalanceUpdateDateTime = DateTime.UtcNow;

            Events.Add(new BrokerAccountBalancesUpdated()
            {
                BrokerAccountId = this.BrokerAccountId,
                AssetId = this.AssetId,
                Sequence = this.Sequence,
                ReservedBalanceUpdateDateTime = this.ReservedBalanceUpdateDateTime,
                AvailableBalance = this.AvailableBalance,
                OwnedBalance = this.OwnedBalance,
                OwnedBalanceUpdateDateTime = this.OwnedBalanceUpdateDateTime,
                AvailableBalanceUpdateDateTime = this.AvailableBalanceUpdateDateTime,
                ReservedBalance = this.ReservedBalance,
                PendingBalanceUpdateDateTime = this.PendingBalanceUpdateDateTime,
                PendingBalance = this.PendingBalance,
                BrokerAccountBalancesId = this.Id
            });
        }

        public void MovePendingBalanceToOwned(decimal ownedBalanceChange)
        {
            PendingBalance -= ownedBalanceChange;
            OwnedBalance += ownedBalanceChange;

            var updateDateTime = DateTime.UtcNow;
            PendingBalanceUpdateDateTime = updateDateTime;
            OwnedBalanceUpdateDateTime = updateDateTime;

            Events.Add(new BrokerAccountBalancesUpdated()
            {
                BrokerAccountId = this.BrokerAccountId,
                AssetId = this.AssetId,
                Sequence = this.Sequence,
                ReservedBalanceUpdateDateTime = this.ReservedBalanceUpdateDateTime,
                AvailableBalance = this.AvailableBalance,
                OwnedBalance = this.OwnedBalance,
                OwnedBalanceUpdateDateTime = this.OwnedBalanceUpdateDateTime,
                AvailableBalanceUpdateDateTime = this.AvailableBalanceUpdateDateTime,
                ReservedBalance = this.ReservedBalance,
                PendingBalanceUpdateDateTime = this.PendingBalanceUpdateDateTime,
                PendingBalance = this.PendingBalance,
                BrokerAccountBalancesId = this.Id
            });
        }

        public void MovePendingBalanceToAvailableAndOwned(decimal pendingChangeAmount, decimal ownedChangeAmount, decimal availableChangeAmount)
        {
            PendingBalance += pendingChangeAmount;
            OwnedBalance += ownedChangeAmount;
            AvailableBalance += availableChangeAmount;

            var updateDateTime = DateTime.UtcNow;
            PendingBalanceUpdateDateTime = updateDateTime;
            OwnedBalanceUpdateDateTime = updateDateTime;

            if (availableChangeAmount != 0)
            {
                AvailableBalanceUpdateDateTime = updateDateTime;
            }

            Events.Add(new BrokerAccountBalancesUpdated()
            {
                BrokerAccountId = this.BrokerAccountId,
                AssetId = this.AssetId,
                Sequence = this.Sequence,
                ReservedBalanceUpdateDateTime = this.ReservedBalanceUpdateDateTime,
                AvailableBalance = this.AvailableBalance,
                OwnedBalance = this.OwnedBalance,
                OwnedBalanceUpdateDateTime = this.OwnedBalanceUpdateDateTime,
                AvailableBalanceUpdateDateTime = this.AvailableBalanceUpdateDateTime,
                ReservedBalance = this.ReservedBalance,
                PendingBalanceUpdateDateTime = this.PendingBalanceUpdateDateTime,
                PendingBalance = this.PendingBalance,
                BrokerAccountBalancesId = this.Id
            });
        }
    }
}
