using System;
using System.Collections.Generic;

namespace Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts
{
    public class BrokerAccountUpdated
    {
        public long BrokerAccountId { get; set; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BrokerAccountState State { get; set; }
        public long VaultId { get; set; }
        public long Sequence { get; set; }

        public IReadOnlyCollection<string> BlockchainIds { get; set; }
    }
}
