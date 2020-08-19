using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.MessagingContract.BrokerAccounts;

namespace Brokerage.GrpcServices
{
    public class BrokerAccountsService : BrokerAccounts.BrokerAccountsBase
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IIdGenerator _idGenerator;

        public BrokerAccountsService(IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IIdGenerator idGenerator)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _idGenerator = idGenerator;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                return new CreateResponse
                {
                    Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                    {
                        ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.NameIsEmpty,
                        ErrorMessage = "Name is empty"
                    }
                };
            }

            try
            {
                await using var unitOfWork = await _unitOfWorkManager.Begin($"BrokerAccounts:Create:{request.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var brokerAccountsId = await _idGenerator.GetId($"BrokerAccounts:{request.RequestId}", IdGenerators.BrokerAccounts);
                    var brokerAccount = BrokerAccount.Create(
                        brokerAccountsId,
                        request.Name,
                        request.TenantId,
                        request.VaultId);

                    await unitOfWork.BrokerAccounts.Add(brokerAccount);

                    unitOfWork.Outbox.Send(new FinalizeBrokerAccountCreation
                    {
                        BrokerAccountId = brokerAccount.Id
                    });

                    foreach (var evt in brokerAccount.Events)
                    {
                        unitOfWork.Outbox.Publish(evt);
                    }

                    unitOfWork.Outbox.Return(new CreateResponse
                    {
                        Response = new CreateResponseBody
                        {
                            Id = brokerAccount.Id,
                            Name = brokerAccount.Name,
                            Status = MapToResponse(brokerAccount.State),
                            CreatedAt = Timestamp.FromDateTime(brokerAccount.CreatedAt),
                            UpdatedAt = Timestamp.FromDateTime(brokerAccount.UpdatedAt),
                            VaultId = brokerAccount.VaultId
                        }
                    });

                    await unitOfWork.Commit();
                }

                await unitOfWork.EnsureOutboxDispatched();

                var response = unitOfWork.Outbox.GetResponse<CreateResponse>();

                if (response.BodyCase == CreateResponse.BodyOneofCase.Error)
                {
                    return response;
                }

                if (response.Response.VaultId != request.VaultId)
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

                return response;
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
