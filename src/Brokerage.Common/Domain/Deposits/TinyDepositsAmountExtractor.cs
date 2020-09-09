using System.Collections.Generic;
using Brokerage.Common.Configuration;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Deposits
{
    public static class TinyDepositsAmountExtractor
    {
        public static decimal GetMinDepositForConsolidation(
            Blockchain blockchain,
            IReadOnlyDictionary<string, BlockchainConfig> blockchainsConfigDict
        )
        {
            var minDepositForConsolidation = 0m;
            if (blockchain.Protocol.Capabilities.DestinationTag == null &&
                blockchainsConfigDict.TryGetValue(blockchain.Id, out var blockchainConfiguration))
            {
                minDepositForConsolidation = blockchainConfiguration.MinDepositForConsolidation;
            }

            return minDepositForConsolidation;
        }
    }
}
