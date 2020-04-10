using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;
using MassTransit;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionDetectedConsumer : IConsumer<TransactionDetected>
    {
        private readonly DepositsDetector _depositsDetector;

        public TransactionDetectedConsumer(DepositsDetector depositsDetector)
        {
            _depositsDetector = depositsDetector;
        }

        public async Task Consume(ConsumeContext<TransactionDetected> context)
        {
            var evt = context.Message;

            await _depositsDetector.Detect(evt);
        }
    }
}
