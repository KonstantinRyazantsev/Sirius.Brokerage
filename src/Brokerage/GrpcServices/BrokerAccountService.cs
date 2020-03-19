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

        public BrokerAccountService(IBrokerAccountRepository brokerAccountRepository)
        {
            _brokerAccountRepository = brokerAccountRepository;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            try
            {
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
                        ErrorCode = 1,
                        ErrorMessage = e.Message,
                    }
                };
            }
        }
    }
}
