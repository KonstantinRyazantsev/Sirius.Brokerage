namespace Brokerage.Common.ReadModels.Assets
{
    public class Asset
    {
        public long Id { get; set; }
        public string BlockchainId { get; set; }
        public string Symbol { get; set; }
        public string Address { get; set; }
        public int Accuracy { get; set; }
    }
}
