using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class AccountRequisitesAdded
    {
        public long AccountRequisitesId { get; set; }
        public long AccountId { get; set; }
        public string BlockchainId { get; set; }
        public string Address { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string Tag { get; set; }
        public TagType? TagType { get; set; }
    }
}
