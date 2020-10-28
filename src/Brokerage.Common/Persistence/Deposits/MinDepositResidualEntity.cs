using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Brokerage.Common.Persistence.Accounts;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence.Deposits
{
    public class MinDepositResidualEntity
    {
        public string NaturalId { get; set; }
        public long AssetId { get; set; }
        public string BlockchainId { get; set; }
        public string Address { get; set; }
        public string Tag { get; set; }
        public DestinationTagType? TagType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public decimal Amount { get; set; }

        public long DepositId { get; set; }

        public long? ConsolidationDepositId { get; set; }

        public long? ProvisioningDepositId { get; set; }

        public uint xmin { get; set; }

    }
}
