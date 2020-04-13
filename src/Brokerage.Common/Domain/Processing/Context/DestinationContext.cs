using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class DestinationContext
    {
        public DestinationContext(string address, string tag, DestinationTagType? tagType,
            Unit unit)
        {
            Address = address;
            Tag = tag;
            TagType = tagType;
            Unit = unit;
        }

        public string Address { get; }
        public string Tag { get; }
        public DestinationTagType? TagType { get; }
        public Unit Unit { get; }
    }
}
