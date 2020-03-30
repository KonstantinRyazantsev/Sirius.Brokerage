using System;
using System.Collections.Generic;
using System.Linq;
using Brokerage.Common.Configuration;
using Lykke.Service.BlockchainApi.Client;
using Microsoft.Extensions.Logging;

namespace Brokerage.Bilv1.DomainServices
{
    public class BlockchainApiClientProvider
    {
        private readonly IDictionary<string, IBlockchainApiClient> _clients;

        public BlockchainApiClientProvider(
            ILoggerFactory loggerFactory,
            IntegrationsConfig integrationsConfig)
        {
            _clients = integrationsConfig.Blockchains
                .ToDictionary(
                    blockchain => blockchain.Id, 
                    blockchain => (IBlockchainApiClient)new BlockchainApiClient(loggerFactory, blockchain.ApiUrl));
        }

        public IBlockchainApiClient Get(string blockchainType)
        {
            if (!_clients.TryGetValue(blockchainType, out var client))
            {
                throw new InvalidOperationException($"Blockchain API client [{blockchainType}] is not found");
            }

            return client;
        }
    }
}
