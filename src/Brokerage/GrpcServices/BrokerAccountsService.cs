﻿using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.ApiContract.Common;

namespace Brokerage.GrpcServices
{
    public class BrokerAccountsService : BrokerAccounts.BrokerAccountsBase
    {
        private readonly IBrokerAccountsRepository brokerAccountsRepository;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public BrokerAccountsService(
            IBrokerAccountsRepository brokerAccountsRepository,
            ISendEndpointProvider sendEndpointProvider)
        {
            this.brokerAccountsRepository = brokerAccountsRepository;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return new CreateResponse
                    {
                        Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                        {
                            ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.NameIsEmpty,
                            ErrorMessage = $"Name is empty"
                        }
                    };
                }

                var newBrokerAccount = BrokerAccount.Create(request.Name, request.TenantId, request.RequestId);
                var createdBrokerAccount = await brokerAccountsRepository.AddOrGetAsync(newBrokerAccount);

                if (!createdBrokerAccount.IsOwnedBy(request.TenantId))
                {
                    return new CreateResponse
                    {
                        Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                        {
                            ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.IsNotAuthorized,
                            ErrorMessage = "Not authorized to perform action"
                        }
                    };
                }

                await _sendEndpointProvider.Send(new FinalizeBrokerAccountCreation
                {
                    BrokerAccountId = createdBrokerAccount.BrokerAccountId,
                    TenantId = createdBrokerAccount.TenantId,
                    RequestId = request.RequestId
                });

                return new CreateResponse
                {
                    Response = new CreateResponseBody
                    {
                        BrokerAccountId = createdBrokerAccount.BrokerAccountId,
                        Name = createdBrokerAccount.Name,
                        Status = MapToResponse(createdBrokerAccount.State),
                        ActivationDateTime = createdBrokerAccount.ActivationDateTime.HasValue ? 
                            Timestamp.FromDateTime(createdBrokerAccount.ActivationDateTime.Value) : null,
                        BlockingDateTime = createdBrokerAccount.BlockingDateTime.HasValue ? 
                            Timestamp.FromDateTime(createdBrokerAccount.BlockingDateTime.Value) : null,
                        CreationDateTime = Timestamp.FromDateTime(createdBrokerAccount.CreationDateTime)
                    }
                };
            }
            catch (Exception e)
            {
                return new CreateResponse
                {
                    Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                    {
                        ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.Unknown,
                        ErrorMessage = e.Message,
                    }
                };
            }
        }

        private static CreateResponseBody.Types.BrokerAccountStatus MapToResponse(BrokerAccountState resultState)
        {
            var result = resultState switch
            {
                BrokerAccountState.Creating => CreateResponseBody.Types.BrokerAccountStatus.Creating,
                BrokerAccountState.Active => CreateResponseBody.Types.BrokerAccountStatus.Active,
                BrokerAccountState.Blocked => CreateResponseBody.Types.BrokerAccountStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null)
            };

            return result;
        }
    }
}
