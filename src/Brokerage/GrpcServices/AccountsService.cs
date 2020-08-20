using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;

namespace Brokerage.GrpcServices
{
    public class AccountsService : Accounts.AccountsBase
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IIdGenerator _idGenerator;

        public AccountsService(IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IIdGenerator idGenerator)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _idGenerator = idGenerator;
        }

        public override async Task<CreateAccountResponse> Create(CreateAccountRequest request, ServerCallContext context)
        {
            try
            {
                await using var unitOfWork = await _unitOfWorkManager.Begin($"Accounts.Create:{request.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var accountId = await _idGenerator.GetId($"Accounts:{request.RequestId}", IdGenerators.Accounts);
                    var account = Account.Create(
                        accountId,
                        request.BrokerAccountId,
                        request.ReferenceId);

                    await unitOfWork.Accounts.Add(account);

                    unitOfWork.Outbox.Send(new FinalizeAccountCreation
                    {
                        AccountId = account.Id
                    });

                    foreach (var evt in account.Events)
                    {
                        unitOfWork.Outbox.Publish(evt);
                    }

                    unitOfWork.Outbox.Return(new CreateAccountResponse
                    {
                        Response = new CreateAccountResponseBody
                        {
                            BrokerAccountId = account.BrokerAccountId,
                            Status = MapToResponse(account.State),
                            Id = account.Id,
                            ReferenceId = account.ReferenceId,
                            UpdatedAt = Timestamp.FromDateTime(account.UpdatedAt),
                            CreatedAt = Timestamp.FromDateTime(account.CreatedAt)
                        }
                    });

                    await unitOfWork.Commit();
                }

                await unitOfWork.EnsureOutboxDispatched();

                var response = unitOfWork.Outbox.GetResponse<CreateAccountResponse>();

                if (response.BodyCase == CreateAccountResponse.BodyOneofCase.Error)
                {
                    return response;
                }

                if (response.Response.BrokerAccountId != request.BrokerAccountId)
                {
                    return new CreateAccountResponse
                    {
                        Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                        {
                            ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                                .Types.ErrorCode.IsNotAuthorized,
                            ErrorMessage = "Not authorized to perform action"
                        }
                    };
                }

                return response;
            }
            catch (Exception e)
            {
                return new CreateAccountResponse
                {
                    Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                    {
                        ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.Unknown,
                        ErrorMessage = e.Message,
                    }
                };
            }
        }

        private static CreateAccountResponseBody.Types.AccountStatus MapToResponse(AccountState resultState)
        {
            var result = resultState switch
            {
                AccountState.Creating => CreateAccountResponseBody.Types.AccountStatus.Creating,
                AccountState.Active => CreateAccountResponseBody.Types.AccountStatus.Active,
                AccountState.Blocked => CreateAccountResponseBody.Types.AccountStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null)
            };

            return result;
        }
    }
}
