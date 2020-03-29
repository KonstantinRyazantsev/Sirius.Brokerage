﻿using System.Threading.Tasks;
using Brokerage.Common.Domain.Blockchains;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract;

namespace Brokerage.Worker.MessageConsumers
{
    public class BlockchainUpdatesConsumer : IConsumer<BlockchainAdded>
    {
        private readonly ILogger<BlockchainUpdatesConsumer> _logger;
        private readonly IBlockchainsRepository blockchainsRepository;

        public BlockchainUpdatesConsumer(
            ILogger<BlockchainUpdatesConsumer> logger, 
            IBlockchainsRepository blockchainsRepository)
        {
            _logger = logger;
            this.blockchainsRepository = blockchainsRepository;
        }

        public async Task Consume(ConsumeContext<BlockchainAdded> context)
        {
            var @event = context.Message;

            var model = new Blockchain
            {
                BlockchainId = context.Message.BlockchainId
            };

            await blockchainsRepository.AddOrReplaceAsync(model);

            _logger.LogInformation("BlockchainAdded command has been processed {@context}", @event);

            await Task.CompletedTask;
        }
    }
}
