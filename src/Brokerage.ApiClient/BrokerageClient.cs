using Swisschain.Sirius.Brokerage.ApiClient.Common;
using Swisschain.Sirius.Brokerage.ApiContract;

namespace Swisschain.Sirius.Brokerage.ApiClient
{
    public class BrokerageClient : BaseGrpcClient, IBrokerageClient
    {
        public BrokerageClient(string serverGrpcUrl) : base(serverGrpcUrl)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }
    }
}
