using System;
using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Accounts
{
    public class AccountDetailsAdded
    {
        public long AccountDetailsId { get; set; }
        public long AccountId { get; set; }
        public string BlockchainId { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Tag { get; set; }
        public DestinationTagType? TagType { get; set; }
    }
}
