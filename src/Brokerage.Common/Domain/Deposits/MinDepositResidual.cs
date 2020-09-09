using System;
using Brokerage.Common.Domain.Accounts;

namespace Brokerage.Common.Domain.Deposits
{
    public class MinDepositResidual
    {
        private MinDepositResidual(
            long depositId,
            decimal amount,
            AccountDetailsId accountDetailsId,
            long assetId,
            long? consolidationDepositId,
            DateTimeOffset createdAt,
            uint version)
        {
            DepositId = depositId;
            Amount = amount;
            AccountDetailsId = accountDetailsId;
            AssetId = assetId;
            ConsolidationDepositId = consolidationDepositId;
            CreatedAt = createdAt;
            Version = version;
        }

        public AccountDetailsId AccountDetailsId { get; private set; }

        public decimal Amount { get; private set; }

        public long DepositId { get; private set; }

        public long AssetId { get; private set; }

        public long? ConsolidationDepositId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public uint Version { get; set; }

        public static MinDepositResidual Create(long depositId, decimal amount, AccountDetailsId accountDetailsId, long assetId)
        {
            var createdAt = DateTimeOffset.UtcNow;
            return new MinDepositResidual(depositId, amount, accountDetailsId, assetId, null, createdAt, default);
        }

        public static MinDepositResidual Restore(long depositId, decimal amount, AccountDetailsId accountDetailsId,
            long assetId,
            long? consolidationDepositId,
            DateTimeOffset createdAt,
            uint version)
        {
            return new MinDepositResidual(
                depositId, 
                amount, 
                accountDetailsId, 
                assetId, 
                consolidationDepositId, 
                createdAt, 
                version);
        }

        public void AddConsolidationDeposit(long depositId)
        {
            ConsolidationDepositId = depositId;
        }
    }
}
