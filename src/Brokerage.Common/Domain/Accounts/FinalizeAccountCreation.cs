namespace Brokerage.Common.Domain.Accounts
{
    public class FinalizeAccountCreation
    {
        public long BrokerAccountId { get; set; }

        public string RequestId { get; set; }

        public long AccountId { get; set; }
        public string TenantId { get; set; }
    }
}
