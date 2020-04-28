using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class DestinationDetails
    {
        public string Address { get; set; }

        public string Tag { get; set; }

        public DestinationTagType? TagType { get; set; }
    }
}
