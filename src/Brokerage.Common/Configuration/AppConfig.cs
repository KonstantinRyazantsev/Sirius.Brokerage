using System.Collections.Generic;

namespace Brokerage.Common.Configuration
{
    public class AppConfig
    {
        public DbConfig Db { get; set; }
        public RabbitMqConfig RabbitMq { get; set; }
        public VaultAgentConfig VaultAgent { get; set; }

        public ExecutorConfig Executor { get; set; }

        public IReadOnlyDictionary<string, BlockchainProtocolConfig> BlockchainProtocols { get; set; }
    }
}
