namespace Brokerage.Bilv1.Domain.Models.Wallets
{
    public sealed class TransferCallbackOptions
    {
        public bool ListenForDetected { get; set; }
        public bool ListenForConfirmationsIncrement { get; set; }
        public bool ListenForConfirmed { get; set; }
        public bool ListenForReverted { get; set; }
    }
}
