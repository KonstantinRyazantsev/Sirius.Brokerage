using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountBalances
    {
        private BrokerAccountBalances(
            long id,
            long version,
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
        }

        public long Id { get; }
        public long Version { get; }
        public long BrokerAccountId { get; }
        public long AssetId { get; }
        public decimal OwnedBalance { get; }
        public decimal AvailableBalance { get; }
        public decimal PendingBalance { get; private set; }
        public decimal ReservedBalance { get; }
        public DateTime OwnedBalanceUpdateDateTime { get; }
        public DateTime AvailableBalanceUpdateDateTime { get; }
        public DateTime PendingBalanceUpdateDateTime { get; private set; }
        public DateTime ReservedBalanceUpdateDateTime { get; }

        public static BrokerAccountBalances Create(
            long brokerAccountId,
            long assetId)
        {
            return new BrokerAccountBalances(
                default,
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
            long version,
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
        }
    }
}
