using System.Collections.Generic;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class AddBlockchainToBrokerAccount
    {
        public long BrokerAccountId { get; set; }

        public IReadOnlyCollection<string> BlockchainIds { get; set; }
    }

    public class AddBlockchainsToAccounts
    {
        public long BrokerAccountId { get; set; }

        public int ExpectedAccountsCount { get; set; }

        public IReadOnlyCollection<string> BlockchainIds { get; set; }

        public long Cursor { get; set; }
    }
}
