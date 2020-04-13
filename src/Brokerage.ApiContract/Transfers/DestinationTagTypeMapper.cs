using System;
using Google.Protobuf.WellKnownTypes;

namespace Swisschain.Sirius.Brokerage.ApiContract.Transfers
{
    public static class DestinationTagTypeMapper
    {
        public static Sdk.Primitives.DestinationTagType ToDomain(DestinationTagType value)
        {
            return value switch
            {
                DestinationTagType.Text => Sdk.Primitives.DestinationTagType.Text,
                DestinationTagType.Number => Sdk.Primitives.DestinationTagType.Number,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static Sdk.Primitives.DestinationTagType? ToDomain(NullableDestinationTagType value)
        {
            return value == null ? 
                default(Sdk.Primitives.DestinationTagType?) : 
                ToDomain(value.Value);
        }

        public static DestinationTagType FromDomain(Sdk.Primitives.DestinationTagType value)
        {
            return value switch
            {
                Sdk.Primitives.DestinationTagType.Text => DestinationTagType.Text,
                Sdk.Primitives.DestinationTagType.Number => DestinationTagType.Number,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static NullableDestinationTagType FromDomain(Sdk.Primitives.DestinationTagType? value)
        {
            return value == null
                ? new NullableDestinationTagType {Null = NullValue.NullValue}
                : new NullableDestinationTagType {Value = FromDomain(value.Value)};
        }
    }
}
