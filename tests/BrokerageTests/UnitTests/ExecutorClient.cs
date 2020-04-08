using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;
using Swisschain.Sirius.Executor.ApiContract.Monitoring;
using Swisschain.Sirius.Executor.ApiContract.Transfers;

namespace BrokerageTests.UnitTests
{
    public class FakeExecutorClient : Swisschain.Sirius.Executor.ApiClient.IExecutorClient
    {
        public Monitoring.MonitoringClient Monitoring { get; }
        public Transfers.TransfersClient Transfers { get; }  = new TestTransfersClient();

        public class TestTransfersClient : Transfers.TransfersClient
        {
            public List<ExecuteTransferRequest> TransferRequests { get; } = new List<ExecuteTransferRequest>();

            public override ExecuteTransferResponse Execute(ExecuteTransferRequest request,
                Metadata headers = null,
                DateTime? deadline = null,
                CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public override ExecuteTransferResponse Execute(ExecuteTransferRequest request, CallOptions options)
            {
                throw new NotImplementedException();
            }

            public override AsyncUnaryCall<ExecuteTransferResponse> ExecuteAsync(ExecuteTransferRequest request,
                Metadata headers = null,
                DateTime? deadline = null,
                CancellationToken cancellationToken = default)
            {
                var fakeCall = TestCalls.AsyncUnaryCall<ExecuteTransferResponse>(
                    Task.FromResult(new ExecuteTransferResponse()), 
                    Task.FromResult(new Metadata()), 
                    () => Status.DefaultSuccess, 
                    () => new Metadata(), 
                    () => { });


                TransferRequests.Add(request);

                return fakeCall;
            }

            public override AsyncUnaryCall<ExecuteTransferResponse> ExecuteAsync(ExecuteTransferRequest request,
                CallOptions options)
            {
                var fakeCall = TestCalls.AsyncUnaryCall<ExecuteTransferResponse>(
                    Task.FromResult(new ExecuteTransferResponse()),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });

                TransferRequests.Add(request);

                return fakeCall;
            }
        }
    }
}
