namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccount
    {
        public string BlockchainId { get; set; }

        public string NetworkId { get; set; }

        public long BrokerAccountId { get; set; }

        public string Name { get; set; }

        public string TenantId { get; set; }
    }
}
