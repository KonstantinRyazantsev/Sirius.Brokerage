using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence;
using Grpc.Core;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.ApiContract.common;

namespace Brokerage.GrpcServices
{
    public class BrokerAccountsService : BrokerAccounts.BrokerAccountsBase
    {
        private readonly IBrokerAccountRepository _brokerAccountRepository;

        public BrokerAccountsService(
            IBrokerAccountRepository brokerAccountRepository)
        {
            _brokerAccountRepository = brokerAccountRepository;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return new CreateResponse()
                    {
                        Error = new ErrorResponseBody()
                        {
                            ErrorCode = ErrorResponseBody.Types.ErrorCode.NameIsEmpty,
                            ErrorMessage = $"Name is empty"
                        }
                    };
                }

                var result = await _brokerAccountRepository.AddOrGetAsync(
                    request.RequestId,
                    request.TenantId,
                    request.Name);

                //TODO: Refactor this 
                if (result == null)
                {
                    return new CreateResponse()
                    {
                        Error = new ErrorResponseBody()
                        {
                            ErrorCode = ErrorResponseBody.Types.ErrorCode.IsNotAuthorized,
                            ErrorMessage = "Not authorized to perform action"
                        }
                    };
                }

                return new CreateResponse()
                {
                    Response = new CreateResponseBody()
                    {
                        BrokerAccountId = result.BrokerAccountId.ToString(),
                        Name = result.Name,
                        Status = MapToResponse(result.State)
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

        private CreateResponseBody.Types.BrokerAccountStatus MapToResponse(BrokerAccountState resultState)
        {
            var result = resultState switch
            {
                BrokerAccountState.Creating => CreateResponseBody.Types.BrokerAccountStatus.Creating,
                BrokerAccountState.Active=> CreateResponseBody.Types.BrokerAccountStatus.Active,
                BrokerAccountState.Blocked => CreateResponseBody.Types.BrokerAccountStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null)
            };

            return result;
        }
    }
}
