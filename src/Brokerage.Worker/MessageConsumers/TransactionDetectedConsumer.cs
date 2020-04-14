using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionDetectedConsumer : IConsumer<TransactionDetected>
    {
        private readonly ILogger<TransactionDetectedConsumer> _logger;
        private readonly DepositsDetector _depositsDetector;

        public TransactionDetectedConsumer(ILogger<TransactionDetectedConsumer> logger,
            DepositsDetector depositsDetector)
        {
            _logger = logger;
            _depositsDetector = depositsDetector;
        }

        public async Task Consume(ConsumeContext<TransactionDetected> context)
        {
            var evt = context.Message;

            await _depositsDetector.Detect(evt);

            _logger.LogInformation("Detected transaction has been processed {@context}", evt);
        }
    }
}
