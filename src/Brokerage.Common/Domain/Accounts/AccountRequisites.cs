using System;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Accounts
{
    public class AccountRequisites
    {
        private AccountRequisites(
            string requestId,
            long accountRequisitesId,
            long accountId,
            BlockchainId blockchainId,
            string address,
            DestinationTag tag,
            DestinationTagType? tagType,
            DateTime creationDate)
        {
            RequestId = requestId;
            AccountRequisitesId = accountRequisitesId;
            AccountId = accountId;
            BlockchainId = blockchainId;
            Address = address;
            Tag = tag;
            TagType = tagType;
            CreationDateTime = creationDate;
        }

        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public long AccountRequisitesId { get; }
        public long AccountId { get; }
        public BlockchainId BlockchainId { get; }
        public DestinationTag Tag { get; }
        public DestinationTagType? TagType { get; }
        public DateTime CreationDateTime { get; }
        public string Address { get; set; }

        public static AccountRequisites Create(
            string requestId,
            long accountId,
            BlockchainId blockchainId,
            string address,
            DestinationTag tag = null,
            DestinationTagType? tagType = null)
        {
            return new AccountRequisites(
                    requestId,
                    default,
                    accountId,
                    blockchainId,
                    address,
                    tag,
                    tagType,
                    DateTime.UtcNow);
        }

        public static AccountRequisites Restore(
            string requestId,
            long accountRequisitesId,
            long accountId,
            BlockchainId blockchainId,
            string address,
            DestinationTag tag,
            DestinationTagType? tagType,
            DateTime creationDateTime)
        {
            return new AccountRequisites(
                requestId,
                accountRequisitesId,
                accountId,
                blockchainId,
                address,
                tag,
                tagType,
                creationDateTime);
        }
    }
}
