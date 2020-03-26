using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.AccountRequisites
{
    public class AccountRequisites
    {
        private AccountRequisites(
            string requestId,
            long accountRequisitesId,
            long accountId,
            string tenantId,
            BlockchainId blockchainId,
            string address,
            string tag,
            TagType? tagType)
        {
            RequestId = requestId;
            AccountRequisitesId = accountRequisitesId;
            AccountId = accountId;
            TenantId = tenantId;
            BlockchainId = blockchainId;
            Address = address;
            Tag = tag;
            TagType = tagType;
        }

        public string RequestId { get; }
        public long AccountRequisitesId { get; }

        public long AccountId { get; }

        public string TenantId { get; }
        public BlockchainId BlockchainId { get; }
        public string Address { get; set; }
        public string Tag { get; }
        public TagType? TagType { get; }

        public bool IsOwnedBy(string tenantId)
        {
            return this.TenantId == tenantId;
        }

        public static AccountRequisites Create(
            string requestId,
            long accountId,
            string tenantId,
            BlockchainId blockchainId,
            string address,
            string tag = null,
            TagType? tagType = null)
        {
            return new AccountRequisites(
                    requestId,
                    default,
                    accountId,
                    tenantId,
                    blockchainId,
                    address,
                    tag,
                    tagType);
        }

        public static AccountRequisites Restore(
            string requestId,
            long accountRequisitesId,
            long accountId,
            string tenantId,
            BlockchainId blockchainId,
            string address,
            string tag = null,
            TagType? tagType = null)
        {
            return new AccountRequisites(
                requestId,
                accountRequisitesId,
                accountId,
                tenantId,
                blockchainId,
                address,
                tag,
                tagType);
        }
    }
}
