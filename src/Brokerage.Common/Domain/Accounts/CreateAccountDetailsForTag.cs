namespace Brokerage.Common.Domain.Accounts
{
    public class CreateAccountDetailsForTag
    {
        public long BrokerAccountId { get; set; }
        public long AccountId { get; set; }
        public string BlockchainId { get; set; }
        public long ExpectedBlockchainsCount { get; set; }
        public long ExpectedAccountsCount { get; set; }
    }
}
