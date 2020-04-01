﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.BlockchainApi.Client;
using Microsoft.Extensions.Logging;

namespace Brokerage.Bilv1.DomainServices
{
    public class BlockchainApiClientProvider
    {
        private readonly IDictionary<string, IBlockchainApiClient> _clients;

        public BlockchainApiClientProvider(
            ILoggerFactory loggerFactory,
            IBlockchainsRepository blockchainsRepository)
        {
            _clients = blockchainsRepository
                .GetAllAsync()
                .GetAwaiter()
                .GetResult()
                .ToDictionary(
                    x => x.BlockchainId,
                    x => x.IntegrationUrl)
                .ToDictionary(
                    x => x.Key,
                    x => (IBlockchainApiClient) new BlockchainApiClient(loggerFactory, x.Value));
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
