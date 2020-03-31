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
        public decimal PendingBalance { get; }
        public decimal ReservedBalance { get; }
        public DateTime OwnedBalanceUpdateDateTime { get; }
        public DateTime AvailableBalanceUpdateDateTime { get; }
        public DateTime PendingBalanceUpdateDateTime { get; }
        public DateTime ReservedBalanceUpdateDateTime { get; }

        public static BrokerAccountBalances Create(
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
                default,
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
    }
}
