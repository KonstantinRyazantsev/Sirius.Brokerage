using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.AccountRequisites
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
            DestinationTagType? tagType)
        {
            RequestId = requestId;
            AccountRequisitesId = accountRequisitesId;
            AccountId = accountId;
            BlockchainId = blockchainId;
            Address = address;
            Tag = tag;
            TagType = tagType;
        }

        // TODO: This is here 
        public string RequestId { get; }
        public long AccountRequisitesId { get; }
        public long AccountId { get; }
        public BlockchainId BlockchainId { get; }
        public string Address { get; set; }
        public DestinationTag Tag { get; }
        public DestinationTagType? TagType { get; }

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
                    tagType);
        }

        public static AccountRequisites Restore(
            string requestId,
            long accountRequisitesId,
            long accountId,
            BlockchainId blockchainId,
            string address,
            DestinationTag tag = null,
            DestinationTagType? tagType = null)
        {
            return new AccountRequisites(
                requestId,
                accountRequisitesId,
                accountId,
                blockchainId,
                address,
                tag,
                tagType);
        }
    }
}
