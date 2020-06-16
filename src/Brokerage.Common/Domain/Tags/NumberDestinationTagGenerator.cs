using System;
using Brokerage.Common.Configuration;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Tags
{
    public class NumberDestinationTagGenerator : IDestinationTagGenerator
    {
        private NumberDestinationTagsCapabilities capabilities;
        private static Random _random = new Random();
        private readonly long _max;

        public NumberDestinationTagGenerator(
            NumberDestinationTagsCapabilities capabilities,
            BlockchainProtocolConfig blockchainProtocolConfig)
        {
            this.capabilities = capabilities;

            if (blockchainProtocolConfig?.DesiredMaxNumberTag == null)
            {
                _max = capabilities.Max;
            }
            else
            {
                _max = blockchainProtocolConfig.DesiredMaxNumberTag.Value;
            }
        }

        public string Generate()
        {
            var result = LongRandom(capabilities.Min, _max).ToString();

            return result;
        }

        private long LongRandom(long min, long max)
        {
            var buf = new byte[8];
            _random.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }
    }
}
