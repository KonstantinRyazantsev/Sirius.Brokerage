using System;
using System.Collections.Generic;
using System.Text;

namespace Brokerage.Common.Domain
{
    public class RequesterContext
    {
        public long AggregateId { get; set; }

        public AggregateType AggregateType { get; set; }
    }

    public enum AggregateType
    {
        BrokerAccount = 0,
        Account = 1
    }
}
