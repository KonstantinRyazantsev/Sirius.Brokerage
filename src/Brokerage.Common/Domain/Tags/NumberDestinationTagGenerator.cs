using System;
using Brokerage.Common.Configuration;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Tags
{
    public class NumberDestinationTagGenerator : IDestinationTagGenerator
    {
        private NumberDestinationTagsCapabilities _number;
        private readonly BlockchainConfig _blockchainConfig;
        private static readonly Random _random = new Random();
        private readonly long _max;

        public NumberDestinationTagGenerator(
            NumberDestinationTagsCapabilities number,
            BlockchainConfig blockchainConfig)
        {
            this._number = number;
            _blockchainConfig = blockchainConfig;

            if (_blockchainConfig?.DesiredMaxNumberTag == null)
            {
                _max = _number.Max;
            }
            else
            {
                _max = _blockchainConfig.DesiredMaxNumberTag.Value;
            }
        }

        public string Generate()
        {
            var result = LongRandom(_number.Min, _number.Max).ToString();

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
