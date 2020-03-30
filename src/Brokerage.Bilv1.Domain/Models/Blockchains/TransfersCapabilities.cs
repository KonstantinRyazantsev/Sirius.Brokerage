namespace Brokerage.Bilv1.Domain.Models.Blockchains
{
    public sealed class TransfersCapabilities
    {
        public bool OneToMany { get; set; }
        public bool ManyToOne { get; set; }
        public bool ManyToMany { get; set; }
        public bool SourceAddressNonce { get; set; }
        public bool AsAtBlock { get; set; }
        public bool ChangeRecipientAddress { get; set; }
        public bool MultipleAssets { get; set; }
        public TransfersDestinationTagCapabilities DestinationTag { get; set; }
    }
}
