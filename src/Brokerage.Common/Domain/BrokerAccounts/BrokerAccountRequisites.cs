using System;

namespace Brokerage.Common.Domain.BrokerAccounts
{
    public class BrokerAccountRequisites
    {
        private BrokerAccountRequisites(
            long id,
            long brokerAccountId,
            string blockchainId,
            string address, 
            string requestId,
            DateTime creationDateTime)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            BlockchainId = blockchainId;
            Address = address;
            RequestId = requestId;
            CreationDateTime = creationDateTime;
        }

        public long Id { get; }
        public long BrokerAccountId { get; }
        // TODO: This is here only because of EF - we can't update DB record without having entire entity
        public string RequestId { get; }
        public string BlockchainId { get; }
        public DateTime CreationDateTime { get; }
        public string Address { get; set; }
        
        public static BrokerAccountRequisites Create(
            string requestId,
            long brokerAccountId,
            string blockchainId)
        {
            return new BrokerAccountRequisites(default, brokerAccountId, blockchainId, null, requestId, DateTime.UtcNow);
        }

        public static BrokerAccountRequisites Restore(string requestId,
            long id, 
            long brokerAccountId, 
            string blockchainId, 
            string address,
            DateTime creationDateTime)
        {
            return new BrokerAccountRequisites(id, brokerAccountId, blockchainId, address, requestId, creationDateTime);
        }
    }
}
