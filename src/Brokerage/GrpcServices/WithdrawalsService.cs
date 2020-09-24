using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.ReadModels.Assets;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.ApiContract;
using Swisschain.Sirius.Brokerage.ApiContract.Common;
using Swisschain.Sirius.Sdk.Crypto.AddressFormatting;
using DestinationDetails = Brokerage.Common.Domain.Withdrawals.DestinationDetails;
using DestinationTagType = Swisschain.Sirius.Sdk.Primitives.DestinationTagType;
using ErrorResponseBody = Swisschain.Sirius.Brokerage.ApiContract.ErrorResponseBody;

namespace Brokerage.GrpcServices
{
    public class WithdrawalsService : Withdrawals.WithdrawalsBase
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IIdGenerator _idGenerator;
        private readonly IAssetsRepository _assetsRepository;
        private readonly ILogger<WithdrawalsService> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IAddressFormatterFactory _addressFormatterFactory;

        public WithdrawalsService(
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IIdGenerator idGenerator,
            IAssetsRepository assetsRepository,
            ILogger<WithdrawalsService> logger,
            IBlockchainsRepository blockchainsRepository,
            IAddressFormatterFactory addressFormatterFactory)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _idGenerator = idGenerator;
            _assetsRepository = assetsRepository;
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _addressFormatterFactory = addressFormatterFactory;
        }

        public override async Task<ExecuteWithdrawalWrapperResponse> Execute(ExecuteWithdrawalRequest request, ServerCallContext context)
        {
            try
            {
                await using var unitOfWork = await _unitOfWorkManager.Begin($"Withdrawals.Execute:{request.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var asset = await _assetsRepository.GetOrDefaultAsync(request.AssetId);
                    var amount = (decimal)request.Amount;
                    var validationError = await ValidateRequest(
                        unitOfWork,
                        request,
                        asset,
                        amount);

                    if (validationError != null)
                    {
                        return validationError;
                    }

                    var brokerAccountDetails = await unitOfWork.BrokerAccountDetails
                        .GetActive(new ActiveBrokerAccountDetailsId(asset.BlockchainId, request.BrokerAccountId));

                    var withdrawalId = await _idGenerator.GetId($"Withdrawals:{request.RequestId}", IdGenerators.Withdrawals);

                    var userContext = request.UserContext != null ? new Common.Domain.Withdrawals.UserContext()
                    {
                        UserId = request.UserContext.UserId,
                        ApiKeyId = request.UserContext.ApiKeyId,
                        WithdrawalReferenceId = request.UserContext.WithdrawalReferenceId,
                        AccountReferenceId = request.UserContext.AccountReferenceId,
                        PassClientIp = request.UserContext.PassClientIp
                    } : new Common.Domain.Withdrawals.UserContext();

                    var withdrawal = Withdrawal.Create(
                        withdrawalId,
                        request.BrokerAccountId,
                        brokerAccountDetails.Id,
                        request.AccountId,
                        new Swisschain.Sirius.Sdk.Primitives.Unit(request.AssetId, amount),
                        request.TenantId,
                        Array.Empty<Swisschain.Sirius.Sdk.Primitives.Unit>(),
                        new DestinationDetails(
                            request.DestinationDetails.Address,
                            request.DestinationDetails.Tag,
                            request.DestinationDetails.TagType.KindCase == NullableDestinationTagType.KindOneofCase.Null
                                ? (DestinationTagType?)null
                                : request.DestinationDetails.TagType.Value switch
                                {
                                    Swisschain.Sirius.Brokerage.ApiContract.Common.DestinationTagType.Text =>
                                    DestinationTagType.Text,
                                    Swisschain.Sirius.Brokerage.ApiContract.Common.DestinationTagType.Number =>
                                    DestinationTagType.Number,
                                    _ => throw new ArgumentOutOfRangeException(
                                        nameof(request.DestinationDetails.TagType.Value),
                                        request.DestinationDetails.TagType,
                                        null)
                                }),
                        userContext);

                    await unitOfWork.Withdrawals.Add(withdrawal);

                    unitOfWork.Outbox.Send(new ExecuteWithdrawal
                    {
                        WithdrawalId = withdrawalId
                    });

                    unitOfWork.Outbox.Return(new ExecuteWithdrawalWrapperResponse
                    {
                        Response = new ExecuteWithdrawalResponse()
                        {
                            Id = withdrawal.Id,
                            Sequence = withdrawal.Sequence,
                            BrokerAccountId = withdrawal.BrokerAccountId,
                            BrokerAccountDetailsId = withdrawal.BrokerAccountDetailsId,
                            TenantId = withdrawal.TenantId,
                            AccountId = withdrawal.AccountId,
                            State = withdrawal.State switch
                            {
                                WithdrawalState.Processing => ExecuteWithdrawalResponse.Types.WithdrawalState
                                    .Processing,
                                WithdrawalState.Executing => ExecuteWithdrawalResponse.Types.WithdrawalState.Executing,
                                WithdrawalState.Sent => ExecuteWithdrawalResponse.Types.WithdrawalState.Sent,
                                WithdrawalState.Completed => ExecuteWithdrawalResponse.Types.WithdrawalState.Completed,
                                WithdrawalState.Failed => ExecuteWithdrawalResponse.Types.WithdrawalState.Failed,
                                _ => throw new ArgumentOutOfRangeException(nameof(withdrawal.State),
                                    withdrawal.State,
                                    null)
                            },
                            CreatedAt = Timestamp.FromDateTime(withdrawal.CreatedAt),
                            UpdatedAt = Timestamp.FromDateTime(withdrawal.UpdatedAt),
                            Unit = new Unit
                            {
                                Amount = withdrawal.Unit.Amount,
                                AssetId = withdrawal.Unit.AssetId
                            },
                            DestinationDetails = new Swisschain.Sirius.Brokerage.ApiContract.DestinationDetails()
                            {
                                Address = withdrawal.DestinationDetails.Address,
                                TagType = withdrawal.DestinationDetails.TagType == null
                                    ? new NullableDestinationTagType() { Null = NullValue.NullValue }
                                    : new NullableDestinationTagType()
                                    {
                                        Value = withdrawal.DestinationDetails.TagType.Value switch
                                        {
                                            DestinationTagType.Text => Swisschain.Sirius.Brokerage.ApiContract.Common
                                                .DestinationTagType.Text,
                                            DestinationTagType.Number => Swisschain.Sirius.Brokerage.ApiContract.Common
                                                .DestinationTagType.Number,
                                            _ => throw new ArgumentOutOfRangeException(
                                                nameof(withdrawal.DestinationDetails.TagType),
                                                withdrawal.DestinationDetails.TagType,
                                                null)
                                        }
                                    },
                                Tag = withdrawal.DestinationDetails.Tag
                            },
                            UserContext = request.UserContext
                        }
                    });

                    foreach (var evt in withdrawal.Events)
                    {
                        unitOfWork.Outbox.Publish(evt);
                    }

                    await unitOfWork.Commit();

                    _logger.LogInformation("Withdrawal execution has been accepted {@context}",
                        new
                        {
                            request,
                            unitOfWork.Outbox.Response
                        });
                }

                await unitOfWork.EnsureOutboxDispatched();

                return unitOfWork.Outbox.GetResponse<ExecuteWithdrawalWrapperResponse>();
            }
            catch (Exception ex)
            {
                return new ExecuteWithdrawalWrapperResponse
                {
                    Error = new ErrorResponseBody
                    {
                        ErrorCode = ErrorResponseBody.Types.ErrorCode.TechnicalProblems,
                        ErrorMessage = ex.Message,
                    }
                };
            }
        }

        private async Task<ExecuteWithdrawalWrapperResponse> ValidateRequest(UnitOfWork unitOfWork,
            ExecuteWithdrawalRequest request,
            Asset asset,
            decimal amount)
        {
            if (string.IsNullOrEmpty(request.TenantId))
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Tenant ID is not specified");
            }

            if (string.IsNullOrEmpty(request.RequestId))
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Request ID is not specified");
            }

            if (string.IsNullOrEmpty(request.DestinationDetails?.Address))
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Destination address is not specified");
            }

            if (amount < 0)
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Amount is not positive");
            }

            var brokerAccount = await unitOfWork.BrokerAccounts.GetOrDefault(request.BrokerAccountId);

            if (brokerAccount == null)
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The broker account doesn't exist");
            }

            if (brokerAccount.TenantId != request.TenantId)
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.Unauthorized, "The Tenant doesn't own specified Broker account");
            }

            if (asset == null)
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "Asset does not exist");
            }

            var blockchain = await _blockchainsRepository.GetOrDefaultAsync(asset.BlockchainId);

            if (blockchain == null)
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "Blockchain is not supported");
            }

            AddressFormat addressFormat;
            try
            {
                var addressFormatter = _addressFormatterFactory.Create(blockchain.Protocol.Code);

                addressFormat = addressFormatter
                    .GetFormats(request.DestinationDetails.Address, blockchain.NetworkType)
                    .FirstOrDefault();
            }
            catch (ArgumentOutOfRangeException)
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "Address formatter is not supported");
            }

            if (string.IsNullOrEmpty(addressFormat?.Address))
            {
                return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Destination address is not valid");
            }

            if (request.AccountId != null)
            {
                var account = await unitOfWork.Accounts.GetOrDefault(request.AccountId.Value);

                if (account == null)
                {
                    return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Account doesn't exist");
                }

                if (account.BrokerAccountId != brokerAccount.Id)
                {
                    return GetErrorResponseExecuteWithdrawalWrapperResponse(ErrorResponseBody.Types.ErrorCode.InvalidParameters, "The Account doesn't relate to the Broker account");
                }
            }

            var balance = await unitOfWork.BrokerAccountBalances.GetOrDefault(new BrokerAccountBalancesId(request.BrokerAccountId, asset.Id));

            var availableBalance = balance?.AvailableBalance ?? 0m;
            if (availableBalance < amount)
            {
                var shortage = amount - availableBalance;

                return GetErrorResponseExecuteWithdrawalWrapperResponse(
                    ErrorResponseBody.Types.ErrorCode.NotEnoughBalance,
                    $"There is no available balance to withdraw {amount} {asset.Symbol}. " +
                    $"Shortage of {shortage} {asset.Symbol}");
            }

            return null;
        }

        private static ExecuteWithdrawalWrapperResponse GetErrorResponseExecuteWithdrawalWrapperResponse(
            ErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new ExecuteWithdrawalWrapperResponse
            {
                Error = new ErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }
}
