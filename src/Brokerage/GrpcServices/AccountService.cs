using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Persistence;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MassTransit;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.ApiContract.common;

namespace Brokerage.GrpcServices
{
    public class AccountService : Accounts.AccountsBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public AccountService(
            IAccountRepository accountRepository,
            ISendEndpointProvider sendEndpointProvider)
        {
            _accountRepository = accountRepository;
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
                var createdAccount = await _accountRepository.AddOrGetAsync(newAccount);

                if (createdAccount.BrokerAccountId != request.BrokerAccountId)
                {
                    return new CreateAccountResponse()
                    {
                        Error = new ErrorResponseBody()
                        {
                            ErrorCode = ErrorResponseBody.Types.ErrorCode.IsNotAuthorized,
                            ErrorMessage = "Not authorized to perform action"
                        }
                    };
                }

                await _sendEndpointProvider.Send(new FinalizeAccountCreation()
                {
                    AccountId = createdAccount.AccountId,
                    RequestId = request.RequestId,

                });

                return new CreateAccountResponse()
                {
                    Response = new CreateAccountResponseBody()
                    {
                        BrokerAccountId = createdAccount.BrokerAccountId,
                        Status = MapToResponse(createdAccount.AccountState),
                        AccountId = createdAccount.AccountId,
                        ReferenceId = createdAccount.ReferenceId,
                        ActivationDateTime = createdAccount.ActivationDateTime.HasValue ? 
                            Timestamp.FromDateTime(createdAccount.ActivationDateTime.Value) : null,
                        BlockingDateTime = createdAccount.BlockingDateTime.HasValue ? 
                            Timestamp.FromDateTime(createdAccount.BlockingDateTime.Value) : null,
                        CreationDateTime = Timestamp.FromDateTime(createdAccount.CreationDateTime)
                    }
                };
            }
            catch (Exception e)
            {
                return new CreateAccountResponse()
                {
                    Error = new ErrorResponseBody()
                    {
                        ErrorCode = ErrorResponseBody.Types.ErrorCode.Unknown,
                        ErrorMessage = e.Message,
                    }
                };
            }
        }

        private CreateAccountResponseBody.Types.AccountStatus MapToResponse(AccountState resultState)
        {
            var result = resultState switch
            {
                AccountState.Creating => CreateAccountResponseBody.Types.AccountStatus.Creating,
                AccountState.Active=>    CreateAccountResponseBody.Types.AccountStatus.Active,
                AccountState.Blocked =>  CreateAccountResponseBody.Types.AccountStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null)
            };

            return result;
        }
    }
}
