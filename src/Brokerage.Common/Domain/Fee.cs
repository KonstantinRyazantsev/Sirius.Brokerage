namespace Brokerage.Common.Domain
{
    public class Fee
    {
        public Fee(long assetId, decimal amount)
        {
            AssetId = assetId;
            Amount = amount;
        }

        public long AssetId { get; }

        public decimal Amount { get; }
    }
}
