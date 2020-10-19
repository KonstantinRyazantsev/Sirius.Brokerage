using System;

namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class RequestContext
    {
        public string UserId { get; set; }

        public string ApiKeyId { get; set; }

        public string Ip { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
