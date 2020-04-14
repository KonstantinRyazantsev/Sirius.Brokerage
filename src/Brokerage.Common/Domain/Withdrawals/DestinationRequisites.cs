﻿using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Withdrawals
{
    public class DestinationRequisites
    {
        public DestinationRequisites(string address, string tag = null, DestinationTagType? tagType = null)
        {
            Address = address;
            Tag = tag;
            TagType = tagType;
        }

        public string Address { get;}

        public string Tag { get; }

        public DestinationTagType? TagType { get; }
    }
}
