namespace Brokerage.Common.Domain.Accounts
{
    public class FinalizeAccountCreationForTag
    {
        public long AccountId { get; set; }

        public string BlockchainId { get; set; }
        public long ExpectedCount { get; set; }
    }
}
