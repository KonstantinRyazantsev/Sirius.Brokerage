namespace Brokerage.Bilv1.Domain.Models.Blockchains
{
    public sealed class TransactionsExpirationCapabilities
    {
        public bool AfterDateTime { get; set; }
        public bool AfterBlockNumber { get; set; }
    }
}
