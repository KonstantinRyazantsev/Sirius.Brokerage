using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence.Accounts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.ApiContract;

namespace Brokerage.GrpcServices
{
    public class AccountsService : Accounts.AccountsBase
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOutboxManager _outboxManager;

        public AccountsService(
            IAccountsRepository accountsRepository,
            IOutboxManager outboxManager)
        {
            _accountsRepository = accountsRepository;
            _outboxManager = outboxManager;
        }

        public override async Task<CreateAccountResponse> Create(CreateAccountRequest request, ServerCallContext context)
        {
            try
            {
                var outbox = await _outboxManager.Open(
                    $"Accounts.Create:{request.RequestId}",
                    () => _accountsRepository.GetNextIdAsync());

                if (!outbox.IsStored)
                {
                    var newAccount = Account.Create(
                        outbox.AggregateId,
                        request.BrokerAccountId,
                        request.ReferenceId);
                    var createdAccount = await _accountsRepository.AddOrGetAsync(newAccount);

                    if (createdAccount.BrokerAccountId != request.BrokerAccountId)
                    {
                        outbox.Return(new CreateAccountResponse
                        {
                            Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                            {
                                ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                                    .Types.ErrorCode.IsNotAuthorized,
                                ErrorMessage = "Not authorized to perform action"
                            }
                        });

                        await _outboxManager.Store(outbox);

                        return outbox.GetResponse<CreateAccountResponse>();
                    }

                    outbox.Send(new FinalizeAccountCreation
                    {
                        AccountId = createdAccount.Id,
                        RequestId = request.RequestId,
                    });

                    foreach (var evt in newAccount.Events)
                    {
                        outbox.Publish(evt);
                    }

                    outbox.Return(new CreateAccountResponse
                    {
                        Response = new CreateAccountResponseBody
                        {
                            BrokerAccountId = createdAccount.BrokerAccountId,
                            Status = MapToResponse(createdAccount.State),
                            Id = createdAccount.Id,
                            ReferenceId = createdAccount.ReferenceId,
                            UpdatedAt = Timestamp.FromDateTime(createdAccount.UpdatedAt),
                            CreatedAt = Timestamp.FromDateTime(createdAccount.CreatedAt)
                        }
                    });

                    await _outboxManager.Store(outbox);
                }

                await _outboxManager.EnsureDispatched(outbox);

                var response = outbox.GetResponse<CreateAccountResponse>();

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
