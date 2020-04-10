using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;
using MassTransit;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class TransactionConfirmedConsumer : IConsumer<TransactionConfirmed>
    {
        private readonly BalanceUpdateConfirmator _balanceUpdateConfirmator;
        private readonly DepositsConfirmator _depositsConfirmator;

        public TransactionConfirmedConsumer(BalanceUpdateConfirmator balanceUpdateConfirmator, DepositsConfirmator depositsConfirmator)
        {
            _balanceUpdateConfirmator = balanceUpdateConfirmator;
            _depositsConfirmator = depositsConfirmator;
        }

        public async Task Consume(ConsumeContext<TransactionConfirmed> context)
        {
            var evt = context.Message;

            await _balanceUpdateConfirmator.Confirm(evt);
            await _depositsConfirmator.Confirm(evt);
        }
    }
}
