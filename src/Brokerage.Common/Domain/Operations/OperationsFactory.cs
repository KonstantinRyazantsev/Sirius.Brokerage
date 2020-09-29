using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;
using Google.Protobuf.WellKnownTypes;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Executor.ApiContract.Common;
using Swisschain.Sirius.Executor.ApiContract.Transfers;
using DestinationTagType = Swisschain.Sirius.Sdk.Primitives.DestinationTagType;
using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;
using UserContext = Swisschain.Sirius.Executor.ApiContract.Transfers.UserContext;

namespace Brokerage.Common.Domain.Operations
{
    internal sealed class OperationsFactory : IOperationsFactory
    {
        private readonly IExecutorClient _executorClient;

        public OperationsFactory(IExecutorClient executorClient)
        {
            _executorClient = executorClient;
        }

        public async Task<Operation> StartDepositConsolidation(string tenantId,
            long depositId,
            string accountAddress,
            string brokerAccountAddress,
            Unit unit,
            long asAtBlockNumber,
            long vaultId,
            string accountReferenceId)
        {
            var response = await _executorClient.Transfers.ExecuteAsync(
                new ExecuteTransferRequest(new ExecuteTransferRequest
                {
                    AssetId = unit.AssetId,
                    Operation = new OperationRequest
                    {
                        AsAtBlockNumber = asAtBlockNumber,
                        RequestId = $"Brokerage:DepositConsolidation:{depositId}",
                        FeePayerAddress = brokerAccountAddress,
                        TenantId = tenantId
                    },
                    Movements =
                    {
                        new Movement
                        {
                            SourceAddress = accountAddress,
                            DestinationAddress = brokerAccountAddress,
                            Amount = unit.Amount,
                        }
                    },
                    Component = nameof(Brokerage),
                    OperationType = "Deposit consolidation",
                    VaultId = vaultId,
                    //TODO: What should we pass here
                    UserContext = new UserContext()
                    {
                        WithdrawalParamsSignature = null,
                        WithdrawalReferenceId = null,
                        UserId = null,
                        AccountReferenceId = accountReferenceId,
                        ApiKeyId = null,
                        PassClientIp = "127.0.0.1"
                    }
                }));

            if (response.BodyCase == ExecuteTransferResponse.BodyOneofCase.Error)
            {
                throw new InvalidOperationException($"Failed to start deposit consolidation {response.Error.ErrorCode} {response.Error.ErrorMessage}");
            }

            return Operation.Create(response.Response.Operation.Id, OperationType.DepositConsolidation);
        }

        public async Task<Operation> StartWithdrawal(string tenantId,
            long withdrawalId,
            string brokerAccountAddress,
            DestinationDetails destinationDetails,
            Unit unit,
            long vaultId,
            Brokerage.Common.Domain.Withdrawals.UserContext userContext)
        {
            var response = await _executorClient.Transfers.ExecuteAsync(new ExecuteTransferRequest
            {
                AssetId = unit.AssetId,
                Operation = new OperationRequest
                {
                    RequestId = $"Brokerage:Withdrawal:{withdrawalId}",
                    TenantId = tenantId,
                    FeePayerAddress = brokerAccountAddress
                },
                Movements =
                {
                    new Movement
                    {
                        Amount = unit.Amount,
                        SourceAddress = brokerAccountAddress,
                        DestinationAddress = destinationDetails.Address,
                        DestinationTag = destinationDetails.Tag,
                        DestinationTagType = destinationDetails.TagType switch {
                            DestinationTagType.Text => new NullableDestinationTagType
                            {
                                Value = Swisschain.Sirius.Executor.ApiContract.Transfers.DestinationTagType.Text
                            },
                            DestinationTagType.Number => new NullableDestinationTagType
                            {
                                Value = Swisschain.Sirius.Executor.ApiContract.Transfers.DestinationTagType.Number
                            },
                            null => new NullableDestinationTagType
                            {
                                Null = NullValue.NullValue
                            },
                            _ => throw new ArgumentOutOfRangeException()
                        }
                    }
                },
                Component = nameof(Brokerage),
                OperationType = "Withdrawal",
                VaultId = vaultId,
                UserContext = new UserContext()
                {
                    AccountReferenceId = userContext.AccountReferenceId,
                    ApiKeyId = userContext.ApiKeyId,
                    WithdrawalReferenceId = userContext.WithdrawalReferenceId,
                    UserId = userContext.UserId,
                    PassClientIp = userContext.PassClientIp,
                    WithdrawalParamsSignature = userContext.WithdrawalParamsSignature
                }
            });

            if (response.BodyCase == ExecuteTransferResponse.BodyOneofCase.Error)
            {
                throw new InvalidOperationException($"Failed to start withdrawal {response.Error.ErrorCode} {response.Error.ErrorMessage}");
            }

            return Operation.Create(response.Response.Operation.Id, OperationType.Withdrawal);
        }
    }
}
