﻿using System;
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

                var newBrokerAccount = BrokerAccount.Create(request.Name, request.TenantId);
                var createdBrokerAccount = await _brokerAccountRepository.AddOrGetAsync(
                    request.RequestId,
                    newBrokerAccount);

                //TODO: Refactor this 
                if (!createdBrokerAccount.IsOwnedBy(request.TenantId))
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
                        BrokerAccountId = createdBrokerAccount.BrokerAccountId.ToString(),
                        Name = createdBrokerAccount.Name,
                        Status = MapToResponse(createdBrokerAccount.State)
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
