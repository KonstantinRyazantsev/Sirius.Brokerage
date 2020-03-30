namespace Brokerage.Bilv1.Domain.Models.Blockchains
{
    public sealed class BlockchainRequirements
    {
        public TransfersRequirements Transfers { get; set; }
        public TransactionsFeeRequirements Fee { get; set; }
    }
}
