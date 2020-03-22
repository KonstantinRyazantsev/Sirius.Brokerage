using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccount
    {
        public long BrokerAccountId { get; set; }

        public string Name { get; set; }

        public string TenantId { get; set; }

        public DateTime CreationDateTime { get; set; }

        public DateTime? BlockedDateTime { get; set; }

        public DateTime? ActivationDateTime { get; set; }

        public BrokerAccountState State { get; set; }
    }
}
