﻿using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.BrokerAccountRequisites
{
    public class BrokerAccountRequisites
    {
        private BrokerAccountRequisites(
            long id,
            long brokerAccountId,
            BlockchainId blockchainId,
            Address address, 
            string requestId)
        {
            Id = id;
            BrokerAccountId = brokerAccountId;
            BlockchainId = blockchainId;
            Address = address;
            RequestId = requestId;
        }

        public long Id { get; }

        public long BrokerAccountId { get; }

        public string RequestId { get; set; }

        public BlockchainId BlockchainId { get; }

        public Address Address { get; set; }

        public static BrokerAccountRequisites Create(
            string requestId,
            long brokerAccountId,
            BlockchainId blockchainId)
        {
            return new BrokerAccountRequisites(default, brokerAccountId, blockchainId, null, requestId);
        }

        public static BrokerAccountRequisites Restore(string requestId,
        long id, 
        long brokerAccountId, 
        BlockchainId blockchainId, 
        Address address)
        {
            return new BrokerAccountRequisites(id, brokerAccountId, blockchainId, address, requestId);
        }
    }
}