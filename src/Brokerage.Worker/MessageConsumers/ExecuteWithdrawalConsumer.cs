using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Withdrawals;
using Brokerage.Common.ReadModels.Assets;
using Google.Protobuf.WellKnownTypes;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Executor.ApiContract.Common;
using Swisschain.Sirius.Executor.ApiContract.Transfers;
using Swisschain.Sirius.Indexer.MessagingContract;
using DestinationTagType = Swisschain.Sirius.Sdk.Primitives.DestinationTagType;

namespace Brokerage.Worker.MessageConsumers
{
    public class ExecuteWithdrawalConsumer : IConsumer<ExecuteWithdrawal>
    {
        private readonly ILogger<ExecuteWithdrawalConsumer> _logger;
        private readonly IExecutorClient _executorClient;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;

        public ExecuteWithdrawalConsumer(
            ILogger<ExecuteWithdrawalConsumer> logger,
            Swisschain.Sirius.Executor.ApiClient.IExecutorClient executorClient,
            IWithdrawalRepository withdrawalRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository)
        {
            _logger = logger;
            _executorClient = executorClient;
            _withdrawalRepository = withdrawalRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
        }

        public async Task Consume(ConsumeContext<ExecuteWithdrawal> context)
        {
            var evt = context.Message;

            var withdrawal = await _withdrawalRepository.GetAsync(evt.WithdrawalId);
            var sourceRequisites =
                await _brokerAccountRequisitesRepository.GetByIdAsync(withdrawal.BrokerAccountRequisitesId);

            var destinationTagType = withdrawal.DestinationRequisites.TagType == null
                ? new NullableDestinationTagType()
                {
                    Null = NullValue.NullValue,
                }
                : new NullableDestinationTagType()
                {
                    Value = withdrawal.DestinationRequisites.TagType.Value switch{
                        DestinationTagType.Text => Swisschain.Sirius.Executor.ApiContract.Transfers.DestinationTagType.Text,
                        DestinationTagType.Number => Swisschain.Sirius.Executor.ApiContract.Transfers.DestinationTagType.Number,
                        _ => throw new ArgumentOutOfRangeException(
                            nameof(withdrawal.DestinationRequisites.TagType), 
                            withdrawal.DestinationRequisites.TagType , 
                            null)
                }
                };
            var operation = await _executorClient.Transfers.ExecuteAsync(new ExecuteTransferRequest()
            {
                AssetId = withdrawal.Unit.AssetId,
                Operation = new OperationRequest()
                {
                    RequestId = $"WORKER:WITHDRAWAL:{withdrawal.Id}",
                    TenantId = withdrawal.TenantId,
                    FeePayerAddress = sourceRequisites.Address
                },
                Movements =
                {
                    new Movement()
                    {
                        Amount = withdrawal.Unit.Amount,
                        //DestinationTagType = destinationTagType,
                        SourceAddress = sourceRequisites.Address,
                        //DestinationTag = withdrawal.DestinationRequisites.Tag,
                        DestinationAddress = withdrawal.DestinationRequisites.Address,
                        //SourceNonce = null
                    }
                }
            });

            withdrawal.AddOperation(operation.Response.Operation.Id);
            await _withdrawalRepository.SaveAsync(withdrawal);

            _logger.LogInformation("Asset has been added {@context}", evt);

            await Task.CompletedTask;
        }
    }
}
