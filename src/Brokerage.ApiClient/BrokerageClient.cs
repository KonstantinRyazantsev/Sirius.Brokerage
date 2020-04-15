using Swisschain.Sirius.Brokerage.ApiClient.Common;
using Swisschain.Sirius.Brokerage.ApiContract;

namespace Swisschain.Sirius.Brokerage.ApiClient
{
    public class BrokerageClient : BaseGrpcClient, IBrokerageClient
    {
        public BrokerageClient(string serverGrpcUrl, bool unencrypted) : base(serverGrpcUrl, unencrypted)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
            BrokerAccounts = new BrokerAccounts.BrokerAccountsClient(Channel);
            Accounts = new Accounts.AccountsClient(Channel);
            Withdrawals = new Withdrawals.WithdrawalsClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }
        public BrokerAccounts.BrokerAccountsClient BrokerAccounts { get; }
        public Accounts.AccountsClient Accounts { get; }
        public Withdrawals.WithdrawalsClient Withdrawals { get; }
    }
}
