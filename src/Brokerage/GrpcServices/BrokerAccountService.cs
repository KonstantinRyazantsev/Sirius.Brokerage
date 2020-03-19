using System;
using System.Threading.Tasks;
using Brokerage.Common.Persistence;
using Grpc.Core;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.ApiContract.common;

namespace Brokerage.GrpcServices
{
    public class BrokerAccountService : BrokerAccount.BrokerAccountBase
    {
        private readonly IBrokerAccountRepository _brokerAccountRepository;
        private readonly INetworkReadModelRepository _networkReadModelRepository;

        public BrokerAccountService(
            IBrokerAccountRepository brokerAccountRepository,
            INetworkReadModelRepository networkReadModelRepository)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _networkReadModelRepository = networkReadModelRepository;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            try
            {
                var existingNetwork =
                    await _networkReadModelRepository.GetOrDefaultAsync(request.BlockchainId, request.NetworkId);

                if (existingNetwork == null)
                {
                    return new CreateResponse()
                    {
                        Error = new ErrorResponseBody()
                        {
                            ErrorCode = ErrorResponseBody.Types.ErrorCode.NotAValidNetwork,
                            ErrorMessage = $"There is no such blockchain - network pair as {request.BlockchainId} - {request.NetworkId}",
                        }
                    };
                }

                var result = await _brokerAccountRepository.AddOrGetAsync(
                    request.RequestId,
                    request.TenantId,
                    request.BlockchainId,
                    request.NetworkId,
                    request.Name);

                return new CreateResponse()
                {
                    Response = new CreateResponseBody()
                    {
                        BrokerAccountId = result.BrokerAccountId.ToString(),
                        BlockchainId = result.BlockchainId,
                        Name = result.Name,
                        NetworkId = result.NetworkId
                    }
                };
            }
            catch (Exception e)
            {
                return new CreateResponse()
                {
                    Error = new ErrorResponseBody()
                    {
                        ErrorCode = ErrorResponseBody.Types.ErrorCode.Unknown,
                        ErrorMessage = e.Message,
                    }
                };
            }
        }
    }
}
