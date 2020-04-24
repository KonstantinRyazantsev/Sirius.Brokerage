using System.Collections.Generic;
using System.Linq;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public static class FeesMath
    {
        public static IReadOnlyDictionary<BrokerAccountBalancesId, decimal> SpreadAcrossBrokerAccounts(
            IEnumerable<Unit> fees, IReadOnlyCollection<long> brokerAccountIds)
        {
            var groupedFees = fees
                .GroupBy(x => x.AssetId)
                .Select(g => new Unit(g.Key, g.Sum(x => x.Amount)))
                .ToArray();

            var brokerAccountFees = groupedFees
                .Select(x => new Unit(x.AssetId, x.Amount / brokerAccountIds.Count))
                .ToArray();

            var brokerAccountFeesShortage = brokerAccountFees
                .GroupBy(x => x.AssetId)
                .Select(g => new Unit(
                    g.Key,
                    groupedFees.Single(x => x.AssetId == g.Key).Amount - g.Sum(x => x.Amount)))
                .ToDictionary(x => x.AssetId, x => x.Amount);

            return brokerAccountIds
                .SelectMany((brokerAccountIndex, brokerAccountId) =>
                {
                    if (brokerAccountIndex == 0)
                    {
                        return brokerAccountFees.Select(x => new
                        {
                            Id = new BrokerAccountBalancesId(brokerAccountId, x.AssetId),
                            Amount = x.Amount + brokerAccountFeesShortage[x.AssetId]
                        });
                    }

                    return brokerAccountFees.Select(x => new
                    {
                        Id = new BrokerAccountBalancesId(brokerAccountId, x.AssetId),
                        Amount = x.Amount + brokerAccountFeesShortage[x.AssetId]
                    });
                })
                .ToDictionary(x => x.Id, x => x.Amount);
        }
    }
}
