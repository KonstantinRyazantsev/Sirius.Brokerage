using Swisschain.Sirius.Brokerage.ApiContract;

namespace Swisschain.Sirius.Brokerage.ApiClient
{
    public interface IBrokerageClient
    {
        Monitoring.MonitoringClient Monitoring { get; }

        BrokerAccounts.BrokerAccountsClient BrokerAccount { get; }
    }
}
