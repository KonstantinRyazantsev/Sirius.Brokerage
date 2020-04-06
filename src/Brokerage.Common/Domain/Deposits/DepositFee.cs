namespace Brokerage.Common.Domain.Deposits
{
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
}
