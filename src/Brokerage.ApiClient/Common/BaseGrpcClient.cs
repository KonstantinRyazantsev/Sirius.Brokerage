using System;
using Grpc.Net.Client;

namespace Swisschain.Sirius.Brokerage.ApiClient.Common
{
    public class BaseGrpcClient : IDisposable
    {
        protected GrpcChannel Channel { get; }

        public BaseGrpcClient(string serverGrpcUrl, bool unencrypted)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", unencrypted);

            Channel = GrpcChannel.ForAddress(serverGrpcUrl);
        }

        public void Dispose()
        {
            Channel?.Dispose();
        }
    }
}
