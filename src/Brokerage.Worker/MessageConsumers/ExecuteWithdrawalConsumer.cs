using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Withdrawals;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Executor.ApiContract.Common;
using Swisschain.Sirius.Executor.ApiContract.Transfers;

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
            IExecutorClient executorClient,
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
            var sourceRequisites = await _brokerAccountRequisitesRepository.GetAsync(withdrawal.BrokerAccountRequisitesId);

            var operation = await _executorClient.Transfers.ExecuteAsync(new ExecuteTransferRequest()
            {
                AssetId = withdrawal.Unit.AssetId,
                Operation = new OperationRequest()
                {
                    RequestId = $"Brokerage:Withdrawal:{withdrawal.Id}",
                    TenantId = withdrawal.TenantId,
                    FeePayerAddress = sourceRequisites.NaturalId.Address
                },
                Movements =
                {
                    new Movement
                    {
                        Amount = withdrawal.Unit.Amount,
                        //DestinationTagType = destinationTagType,
                        SourceAddress = sourceRequisites.NaturalId.Address,
                        //DestinationTag = withdrawal.DestinationRequisites.Tag,
                        DestinationAddress = withdrawal.DestinationRequisites.Address,
                        //SourceNonce = null
                    }
                }
            });

            withdrawal.AddOperation(operation.Response.Operation.Id);
            await _withdrawalRepository.SaveAsync(withdrawal);

            foreach (var @event in withdrawal.Events)
            {
                await context.Publish(@event);
            }

            _logger.LogInformation("ExecuteWithdrawal has been processed {@context}", evt);

            await Task.CompletedTask;
        }
    }
}
