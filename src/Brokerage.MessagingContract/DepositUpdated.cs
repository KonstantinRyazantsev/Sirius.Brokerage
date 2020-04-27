using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Swisschain.Sirius.Brokerage.MessagingContract
{
    public class DepositUpdated
    {
        public long DepositId { get; set; }
        public long Sequence { get; set; }
        public string TenantId { get; set; }
        public string BlockchainId { get; set; }
        public long BrokerAccountId { get; set; }
        public long BrokerAccountRequisitesId { get; set; }
        public long? AccountRequisitesId { get; set; }
        public Unit Unit { get; set; }
        public IReadOnlyCollection<Unit> Fees { get; set; }
        public TransactionInfo TransactionInfo { get; set; }
        public DepositError Error { get; set; }
        public IReadOnlyCollection<DepositSource> Sources { get; set; }
        public DepositState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
