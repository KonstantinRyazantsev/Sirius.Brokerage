using System;
using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class AccountRequisitesAdded
    {
        public long AccountRequisitesId { get; set; }
        public long AccountId { get; set; }
        public string BlockchainId { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Tag { get; set; }
        public DestinationTagType? TagType { get; set; }
    }
}
