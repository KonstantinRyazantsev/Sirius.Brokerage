using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence.Accounts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using Swisschain.Sirius.Brokerage.ApiContract;

namespace Brokerage.GrpcServices
{
    public class AccountsService : Accounts.AccountsBase
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public AccountsService(
            IAccountsRepository accountsRepository,
            ISendEndpointProvider sendEndpointProvider)
        {
            this._accountsRepository = accountsRepository;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public override async Task<CreateAccountResponse> Create(CreateAccountRequest request, ServerCallContext context)
        {
            try
            {
                var newAccount = Account.Create(
                    request.RequestId, 
                    request.BrokerAccountId, 
                    request.ReferenceId);
                var createdAccount = await _accountsRepository.AddOrGetAsync(newAccount);

                if (createdAccount.BrokerAccountId != request.BrokerAccountId)
                {
                    return new CreateAccountResponse
                    {
                        Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                        {
                            ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.IsNotAuthorized,
                            ErrorMessage = "Not authorized to perform action"
                        }
                    };
                }

                await _sendEndpointProvider.Send(new FinalizeAccountCreation
                {
                    AccountId = createdAccount.AccountId,
                    RequestId = request.RequestId,

                });

                return new CreateAccountResponse
                {
                    Response = new CreateAccountResponseBody
                    {
                        BrokerAccountId = createdAccount.BrokerAccountId,
                        Status = MapToResponse(createdAccount.State),
                        AccountId = createdAccount.AccountId,
                        ReferenceId = createdAccount.ReferenceId,
                        UpdatedAt = Timestamp.FromDateTime(createdAccount.UpdatedAt),
                        CreatedAt = Timestamp.FromDateTime(createdAccount.CreatedAt)
                    }
                };
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
                AccountState.Active =>    CreateAccountResponseBody.Types.AccountStatus.Active,
                AccountState.Blocked =>  CreateAccountResponseBody.Types.AccountStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null)
            };

            return result;
        }
    }
}
