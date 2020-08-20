using System;
using System.Linq;
using System.Text;
using Brokerage.Common.Configuration;
using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Tags
{
    public class TextDestinationTagGenerator : IDestinationTagGenerator
    {
        private static readonly char[] Alphabet = Enumerable
            .Range('a', 26)
            .Select(x => (char) x)
            .Concat(Enumerable.Range('0', 10).Select(x => (char) x))
            .ToArray();
        private static readonly Random Random = new Random();

        private readonly long _max;

        public TextDestinationTagGenerator(
            TextDestinationTagsCapabilities text,
            BlockchainProtocolConfig blockchainProtocolConfig)
        {
            if (blockchainProtocolConfig?.DesiredTextTagLength == null)
            {
                _max = text.MaxLength;
            }
            else
            {
                _max = blockchainProtocolConfig.DesiredTextTagLength.Value;
            }
        }

        public string Generate()
        {
            var nextBytes = new byte[_max];
            Random.NextBytes(nextBytes);

            var chars = new char[_max];
            var rd = new Random();

            for (var i = 0; i < _max; i++)
            {
                chars[i] = Alphabet[rd.Next(0, Alphabet.Length)];
            }

            return new string(chars);
        }
    }
}
