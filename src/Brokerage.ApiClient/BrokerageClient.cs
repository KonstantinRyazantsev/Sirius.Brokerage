using Swisschain.Sirius.Brokerage.ApiClient.Common;
using Swisschain.Sirius.Brokerage.ApiContract;

namespace Swisschain.Sirius.Brokerage.ApiClient
{
    public class BrokerageClient : BaseGrpcClient, IBrokerageClient
    {
        public BrokerageClient(string serverGrpcUrl, bool unencrypted) : base(serverGrpcUrl, unencrypted)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
            BrokerAccount = new BrokerAccount.BrokerAccountClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }

        public BrokerAccount.BrokerAccountClient BrokerAccount { get; }
    }
}
