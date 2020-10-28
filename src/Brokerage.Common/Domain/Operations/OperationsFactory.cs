using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.ReadModels.Blockchains;
using Google.Protobuf.WellKnownTypes;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Executor.ApiContract.Common;
using Swisschain.Sirius.Executor.ApiContract.Transfers;
using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;

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
            Swisschain.Sirius.Sdk.Primitives.Unit unit,
            long asAtBlockNumber,
            long vaultId,
            string accountReferenceId,
            long brokerAccountId,
            Blockchain blockchain)
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
                    VaultId = vaultId,
                    TransferContext = new Swisschain.Sirius.Executor.ApiContract.Transfers.TransferContext
                    {
                        AccountReferenceId = accountReferenceId,
                        WithdrawalReferenceId = null,
                        Component = nameof(Brokerage),
                        OperationType = "Deposit consolidation",
                        SourceGroup = brokerAccountId.ToString(),
                        DestinationGroup = brokerAccountId.ToString(),
                        Document = null,
                        Signature = null,
                        //TODO: What should we pass here
                        RequestContext = new Swisschain.Sirius.Executor.ApiContract.Transfers.RequestContext
                        {
                            UserId = null,
                            ApiKeyId = null,
                            Ip = "127.0.0.1",
                            Timestamp = DateTime.UtcNow.ToTimestamp()
                        }
                    }
                }));

            if (response.BodyCase == ExecuteTransferResponse.BodyOneofCase.Error)
            {
                throw new InvalidOperationException($"Failed to start deposit consolidation {response.Error.ErrorCode} {response.Error.ErrorMessage}");
            }

            return Operation.Create(response.Response.Operation.Id, blockchain.Id, OperationType.DepositConsolidation);
        }

        public async Task<Operation> StartDepositProvisioning(string tenantId,
            long depositId,
            string accountAddress,
            string brokerAccountAddress,
            Unit unit,
            long asAtBlockNumber,
            long vaultId,
            string accountReferenceId,
            long brokerAccountId,
            Blockchain blockchain)
        {
            var movement = new Movement
            {
                SourceAddress = brokerAccountAddress,
                DestinationAddress = accountAddress,
                Amount = unit.Amount,
            };

            var estimationResponse = await _executorClient.Transfers.EstimateAsync(new EstimateTransferRequest()
            {
                AssetId = unit.AssetId,
                VaultId = vaultId,
                TenantId = tenantId,
                FeePayerAddress = brokerAccountAddress,
                Movements = { movement },
                RequestId = $"Brokerage:DepositProvisioning:{depositId}"
            });

            if (estimationResponse.BodyCase == EstimateTransferResponse.BodyOneofCase.Error)
            {
                throw new InvalidOperationException(
                    $"Failed to start deposit provisioning {estimationResponse.Error.ErrorCode} {estimationResponse.Error.ErrorMessage}");
            }

            var response = await _executorClient.Transfers.ExecuteAsync(
                new ExecuteTransferRequest(new ExecuteTransferRequest
                {
                    AssetId = unit.AssetId,
                    Operation = new OperationRequest
                    {
                        AsAtBlockNumber = asAtBlockNumber,
                        RequestId = $"Brokerage:DepositProvisioning:{depositId}",
                        FeePayerAddress = brokerAccountAddress,
                        TenantId = tenantId,
                        Fees = { estimationResponse.Response.Fees }
                    },
                    Movements =
                    {
                        movement
                    },
                    VaultId = vaultId,
                    TransferContext = new Swisschain.Sirius.Executor.ApiContract.Transfers.TransferContext
                    {
                        AccountReferenceId = accountReferenceId,
                        WithdrawalReferenceId = null,
                        Component = nameof(Brokerage),
                        OperationType = "Deposit provisioning",
                        SourceGroup = brokerAccountId.ToString(),
                        DestinationGroup = brokerAccountId.ToString(),
                        Document = null,
                        Signature = null,
                        //TODO: What should we pass here
                        RequestContext = new Swisschain.Sirius.Executor.ApiContract.Transfers.RequestContext
                        {
                            UserId = null,
                            ApiKeyId = null,
                            Ip = "127.0.0.1",
                            Timestamp = DateTime.UtcNow.ToTimestamp()
                        }
                    }
                }));

            if (response.BodyCase == ExecuteTransferResponse.BodyOneofCase.Error)
            {
                throw new InvalidOperationException($"Failed to start deposit provisioning {response.Error.ErrorCode} {response.Error.ErrorMessage}");
            }

            return Operation.Create(response.Response.Operation.Id, blockchain.Id, OperationType.DepositProvisioning);
        }

        public async Task<Operation> StartWithdrawal(string tenantId,
            long withdrawalId,
            string brokerAccountAddress,
            DestinationDetails destinationDetails,
            Swisschain.Sirius.Sdk.Primitives.Unit unit,
            long vaultId,
            Withdrawals.TransferContext transferContext,
            string sourceGroup,
            string destinationGroup,
            Blockchain blockchain)
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
                            Swisschain.Sirius.Sdk.Primitives.DestinationTagType.Text => new NullableDestinationTagType
                            {
                                Value = DestinationTagType.Text
                            },
                            Swisschain.Sirius.Sdk.Primitives.DestinationTagType.Number => new NullableDestinationTagType
                            {
                                Value = DestinationTagType.Number
                            },
                            null => new NullableDestinationTagType
                            {
                                Null = NullValue.NullValue
                            },
                            _ => throw new ArgumentOutOfRangeException()
                        }
                    }
                },
                VaultId = vaultId,
                TransferContext = new Swisschain.Sirius.Executor.ApiContract.Transfers.TransferContext
                {
                    AccountReferenceId = transferContext.AccountReferenceId,
                    WithdrawalReferenceId = transferContext.WithdrawalReferenceId,
                    Component = nameof(Brokerage),
                    OperationType = "Withdrawal",
                    SourceGroup = sourceGroup,
                    DestinationGroup = destinationGroup,
                    Document = transferContext.Document,
                    Signature = transferContext.Signature,
                    RequestContext = new Swisschain.Sirius.Executor.ApiContract.Transfers.RequestContext
                    {
                        UserId = transferContext.RequestContext.UserId,
                        ApiKeyId = transferContext.RequestContext.ApiKeyId,
                        Ip = transferContext.RequestContext.Ip,
                        Timestamp = transferContext.RequestContext.Timestamp.ToTimestamp()
                    }
                }
            });

            if (response.BodyCase == ExecuteTransferResponse.BodyOneofCase.Error)
            {
                throw new InvalidOperationException($"Failed to start withdrawal {response.Error.ErrorCode} {response.Error.ErrorMessage}");
            }

            return Operation.Create(response.Response.Operation.Id, blockchain.Id, OperationType.Withdrawal);
        }
    }
}
