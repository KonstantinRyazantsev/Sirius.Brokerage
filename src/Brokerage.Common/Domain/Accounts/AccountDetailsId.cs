﻿using System;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Accounts
{
    /// <summary>
    /// Natural ID of the account details
    /// </summary>
    public sealed class AccountDetailsId : IEquatable<AccountDetailsId>
    {
        public AccountDetailsId(string blockchainId, string address, string tag, DestinationTagType? tagType)
        {
            BlockchainId = blockchainId;
            Address = address;
            Tag = tag;
            TagType = tagType;
        }

        public AccountDetailsId(string blockchainId, string address) :
            this(blockchainId, address, default, default)
        {
        }

        public string BlockchainId { get; }
        public string Address { get; }
        public string Tag { get; }
        public DestinationTagType? TagType { get; }

        public bool Equals(AccountDetailsId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Address == other.Address && Tag == other.Tag && TagType == other.TagType;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is AccountDetailsId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Address, Tag, TagType);
        }

        public override string ToString()
        {
            if (Tag == null)
            {
                return $"{BlockchainId}-{Address}";
            }

            return $"{BlockchainId}-{Address}-{TagType}-{Tag}";
        }
    }
}
