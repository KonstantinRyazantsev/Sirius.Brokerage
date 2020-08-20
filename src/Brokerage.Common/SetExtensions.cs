using System.Collections.Generic;
using System.Linq;

namespace Brokerage.Common
{
    public static class SetExtensions
    {
        public static int AddRange<T>(this ISet<T> set, IEnumerable<T> items)
        {
            return items.Count(set.Add);
        }
    }
}
