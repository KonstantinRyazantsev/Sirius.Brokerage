using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionConfirmedConsumer : IConsumer<TransactionConfirmed>
    {
        private readonly ILogger<TransactionConfirmedConsumer> _logger;
        private readonly BalanceUpdateConfirmator _balanceUpdateConfirmator;
        private readonly DepositsConfirmator _depositsConfirmator;

        public TransactionConfirmedConsumer(ILogger<TransactionConfirmedConsumer> logger, 
            BalanceUpdateConfirmator balanceUpdateConfirmator, 
            DepositsConfirmator depositsConfirmator)
        {
            _logger = logger;
            _balanceUpdateConfirmator = balanceUpdateConfirmator;
            _depositsConfirmator = depositsConfirmator;
        }

        public async Task Consume(ConsumeContext<TransactionConfirmed> context)
        {
            var evt = context.Message;

            await _balanceUpdateConfirmator.Confirm(evt);
            await _depositsConfirmator.Confirm(evt);

            _logger.LogInformation("Confirmed transaction has been processed {@context}", evt);
        }
    }
}
