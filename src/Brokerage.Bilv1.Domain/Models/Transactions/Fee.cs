namespace Brokerage.Bilv1.Domain.Models.Transactions
{
    public sealed class Fee
    {
        public string AssetId { get; set; }
        public decimal? Amount { get; set; }
    }
}
