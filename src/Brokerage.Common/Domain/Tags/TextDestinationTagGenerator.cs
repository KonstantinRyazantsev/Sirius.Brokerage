using System;
using System.Text;
using Brokerage.Common.Configuration;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Tags
{
    public class TextDestinationTagGenerator : IDestinationTagGenerator
    {

        private static readonly Random _random = new Random();
        private readonly long _max;

        public TextDestinationTagGenerator(
            TextDestinationTagsCapabilities text,
            BlockchainConfig blockchainConfig)
        {
            if (blockchainConfig?.DesiredTextTagLength == null)
            {
                _max = text.MaxLength;
            }
            else
            {
                _max = blockchainConfig.DesiredTextTagLength.Value;
            }
        }

        public string Generate()
        {
            var nextBytes = new byte[_max];
            _random.NextBytes(nextBytes);
            var generated = Encoding.UTF8.GetString(nextBytes);

            return generated;
        }
    }
}
