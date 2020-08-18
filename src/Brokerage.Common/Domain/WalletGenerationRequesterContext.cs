namespace Brokerage.Common.Domain
{
    public class WalletGenerationRequesterContext
    {
        public long AggregateId { get; set; }
        public AggregateType AggregateType { get; set; }
        public long ExpectedCount { get; set; }
    }
}
