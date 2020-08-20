using System.Collections.Generic;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountDetailsIdComparer : IEqualityComparer<BrokerAccountDetails>
    {
        public bool Equals(BrokerAccountDetails x, BrokerAccountDetails y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(BrokerAccountDetails obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
