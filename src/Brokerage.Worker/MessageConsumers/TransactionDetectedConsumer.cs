using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Operations;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionDetectedConsumer : IConsumer<TransactionDetected>
    {
        private readonly IProcessorsProvider _processorsProvider;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IOutboxManager _outboxManager;

        public TransactionDetectedConsumer(IProcessorsProvider processorsProvider,
            IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IOperationsRepository operationsRepository,
            IOutboxManager outboxManager)
        {
            _processorsProvider = processorsProvider;
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _operationsRepository = operationsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<TransactionDetected> context)
        {
            var tx = context.Message;
            var processingContextBuilder = new ProcessingContextBuilder(
                _accountRequisitesRepository, 
                _brokerAccountRequisitesRepository, 
                _brokerAccountsBalancesRepository, 
                _operationsRepository, 
                _outboxManager);
            var processingContext = await processingContextBuilder.Build(
                tx.BlockchainId,
                tx.OperationId,
                // TODO: Required confirmations count
                // TODO: Add timestamp to the tx event
                new TransactionInfo(tx.TransactionId, tx.BlockNumber, -1, DateTime.UtcNow),
                tx.Sources
                    .Select(x => new SourceContext(x.Address, x.Unit))
                    .ToArray(),
                tx.Destinations
                    .Select(x => new DestinationContext(
                        x.Address,
                        x.Tag,
                        x.TagType,
                        x.Unit))
                    .ToArray());

            foreach (var processor in _processorsProvider.DetectedTransactionProcessors)
            {
                await processor.Process(tx, processingContext);
            }

            _logger.LogInformation("Detected transaction has been processed {@context}", evt);
        }
    }
}
