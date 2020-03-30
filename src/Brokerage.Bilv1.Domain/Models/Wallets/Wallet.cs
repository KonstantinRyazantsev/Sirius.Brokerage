using System;
using System.Collections.Generic;

namespace Brokerage.Bilv1.Domain.Models.Wallets
{
    public sealed class Wallet
    {
        // TODO: Should be sequential guid
        public Guid Id { get; set; }
        public DateTime ImportedDateTime { get; set; }
        public string Address { get; set; }
        public string PubKey { get; set; }
        public TransferCallbackOptions TransferCallbackOptions { get; set; }
        public bool IsCompromised { get; set; }
    }

    public sealed class WalletBalancesDomain
    {
        public Guid WalletId { get; set; }
        public string Address { get; set; }
        public DateTime AtDateTime { get; set; }
        public long AtBlockNumber { get; set; }
        public IReadOnlyCollection<WalletAssetBalanceDomain> Balances { get; set; }
        public IReadOnlyCollection<WalletAssetBalanceDomain> UnconfirmedBalances { get; set; }
    }

    public sealed class WalletAssetBalanceDomain
    {
        public string AssetId { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public long LastUpdateBlock { get; set; }
    }
}
