namespace Brokerage.Common.Domain
{
    public class WalletGenerationRequesterContext
    {
        public long AggregateId { get; set; }

        public long RootId { get; set; }
        public WalletGenerationReason WalletGenerationReason { get; set; }
        public long ExpectedBlockchainsCount { get; set; }

        public int ExpectedAccountsCount { get; set; }
    }
}
