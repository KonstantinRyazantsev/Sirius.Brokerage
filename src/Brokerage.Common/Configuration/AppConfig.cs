namespace Brokerage.Common.Configuration
{
    public class AppConfig
    {
        public DbConfig Db { get; set; }
        public RabbitMqConfig RabbitMq { get; set; }
        public VaultAgentConfig VaultAgent { get; set; }
        public IntegrationsConfig Integrations { get; set; }
    }
}
