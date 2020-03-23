using System;
using System.Collections.Generic;
using System.Text;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class FinalizeBrokerAccountCreation
    {
        public long BrokerAccountId { get; set; }

        public string RequestId { get; set; }

        public string TenantId { get; set; }
    }
}
