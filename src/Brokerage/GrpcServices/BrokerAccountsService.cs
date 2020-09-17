using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Blockchains;
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
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IIdGenerator _idGenerator;

        public BrokerAccountsService(IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IBlockchainsRepository blockchainsRepository,
            IIdGenerator idGenerator)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _blockchainsRepository = blockchainsRepository;
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
                    var blockchainIds = request.BlockchainIds.ToArray();

                    var blockchains = await _blockchainsRepository.GetByIds(blockchainIds);

                    if (blockchains.Count != blockchainIds.Length)
                    {
                        var notSupportedIds = blockchainIds.Except(blockchains.Select(x => x.Id)).ToArray();
                        var joinedString = string.Join(',', notSupportedIds);
                        var errorResponse = new CreateResponse
                        {
                            Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                            {
                                ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.InvalidParameters,
                                ErrorMessage = $"Given list of blockchains contains unsupported blockchain {joinedString}"
                            }
                        };

                        unitOfWork.Outbox.Return(errorResponse);
                        await unitOfWork.Commit();

                        return errorResponse;
                    }

                    var brokerAccountsId = await _idGenerator.GetId($"BrokerAccounts:{request.RequestId}", IdGenerators.BrokerAccounts);
                    var brokerAccount = BrokerAccount.Create(
                        brokerAccountsId,
                        request.Name,
                        request.TenantId,
                        request.VaultId,
                        blockchainIds);

                    await unitOfWork.BrokerAccounts.Add(brokerAccount);

                    foreach (var command in brokerAccount.Commands)
                    {
                        unitOfWork.Outbox.Send(command);
                    }

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
                            VaultId = brokerAccount.VaultId,
                            BlockchainIds = { brokerAccount.BlockchainIds },
                            Sequence = brokerAccount.Sequence
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

        public override async Task<AddBlockchainResponse> AddBlockchain(AddBlockchainRequest request, ServerCallContext context)
        {
            try
            {
                await using var unitOfWork = await _unitOfWorkManager.Begin($"BrokerAccounts:AddBlockchain:{request.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var blockchainIds = request.BlockchainIds.ToArray();

                    var blockchains = await _blockchainsRepository.GetByIds(blockchainIds);

                    if (blockchains.Count != blockchainIds.Length)
                    {
                        var notSupportedIds = blockchainIds.Except(blockchains.Select(x => x.Id)).ToArray();
                        var joinedString = string.Join(',', notSupportedIds);

                        var errorResponse = new AddBlockchainResponse
                        {
                            Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                            {
                                ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.InvalidParameters,
                                ErrorMessage = $"Given list of blockchains contains unsupported blockchain {joinedString}"
                            }
                        };

                        unitOfWork.Outbox.Return(errorResponse);
                        await unitOfWork.Commit();

                        return errorResponse;
                    }

                    var brokerAccount = await unitOfWork.BrokerAccounts.GetOrDefault(request.BrokerAccountId);

                    if (brokerAccount == null)
                    {
                        var errorResponse = new AddBlockchainResponse
                        {
                            Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                            {
                                ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.IsNotFound,
                                ErrorMessage = $"Broker account with id {request.BrokerAccountId} is not found"
                            }
                        };

                        unitOfWork.Outbox.Return(errorResponse);
                        await unitOfWork.Commit();

                        return errorResponse;
                    }

                    if (brokerAccount.TenantId != request.TenantId)
                    {
                        var errorResponse = new AddBlockchainResponse
                        {
                            Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                            {
                                ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.IsNotAuthorized,
                                ErrorMessage = "Not authorized to perform action"
                            }
                        };

                        unitOfWork.Outbox.Return(errorResponse);
                        await unitOfWork.Commit();

                        return errorResponse;
                    }

                    if (brokerAccount.State == BrokerAccountState.Active)
                    {
                        brokerAccount.AddBlockchain(request.BlockchainIds);
                    }
                    else
                    {
                        var errorResponse = new AddBlockchainResponse
                        {
                            Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                            {
                                ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.BrokerAccountInactive,
                                ErrorMessage = $"Broker account expected to be in {BrokerAccountState.Active}, " +
                                               $"current state is {brokerAccount.State}"
                            }
                        };

                        await unitOfWork.Rollback();

                        return errorResponse;
                    }

                    await unitOfWork.BrokerAccounts.Update(brokerAccount);

                    foreach (var command in brokerAccount.Commands)
                    {
                        unitOfWork.Outbox.Send(command);
                    }

                    foreach (var evt in brokerAccount.Events)
                    {
                        unitOfWork.Outbox.Publish(evt);
                    }

                    unitOfWork.Outbox.Return(new AddBlockchainResponse
                    {
                        Response = new AddBlockchainResponseBody()
                        {
                            VaultId = brokerAccount.VaultId,
                            CreatedAt = Timestamp.FromDateTime(brokerAccount.CreatedAt),
                            UpdatedAt = Timestamp.FromDateTime(brokerAccount.UpdatedAt),
                            Name = brokerAccount.Name,
                            BlockchainIds = { brokerAccount.BlockchainIds },
                            Id = brokerAccount.Id,
                            Status = MapToResponse(brokerAccount.State),
                            Sequence = brokerAccount.Sequence
                        }
                    });

                    await unitOfWork.Commit();
                }

                await unitOfWork.EnsureOutboxDispatched();

                var response = unitOfWork.Outbox.GetResponse<AddBlockchainResponse>();

                return response;
            }
            catch (Exception e)
            {
                return new AddBlockchainResponse
                {
                    Error = new Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody
                    {
                        ErrorCode = Swisschain.Sirius.Brokerage.ApiContract.Common.ErrorResponseBody.Types.ErrorCode.Unknown,
                        ErrorMessage = e.Message,
                    }
                };
            }
        }

        private static BrokerAccountStatus MapToResponse(BrokerAccountState resultState)
        {
            var result = resultState switch
            {
                BrokerAccountState.Creating => BrokerAccountStatus.Creating,
                BrokerAccountState.Active => BrokerAccountStatus.Active,
                BrokerAccountState.Blocked => BrokerAccountStatus.Blocked,
                BrokerAccountState.Updating => BrokerAccountStatus.Updating,
                _ => throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null)
            };

            return result;
        }
    }
}
