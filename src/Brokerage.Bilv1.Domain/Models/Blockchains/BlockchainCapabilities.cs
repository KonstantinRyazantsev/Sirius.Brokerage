namespace Brokerage.Bilv1.Domain.Models.Blockchains
{
    public sealed class BlockchainCapabilities
    {
        public TransfersCapabilities Transfers { get; set; }
        public TransactionsExpirationCapabilities TransactionsExpiration { get; set; }
        
    }
}
