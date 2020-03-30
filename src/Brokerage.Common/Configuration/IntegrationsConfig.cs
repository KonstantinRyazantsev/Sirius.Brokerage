using System;

namespace Brokerage.Common.Configuration
{
    public class IntegrationsConfig
    {
        public BlockchainConfig[] Blockchains { get; set; }
        public TimeSpan BalanceUpdatePeriod { get; set; }
    }
}
