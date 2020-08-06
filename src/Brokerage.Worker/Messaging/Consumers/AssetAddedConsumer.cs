using System.Threading.Tasks;
using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.ReadModels.Assets;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class AssetAddedConsumer : IConsumer<AssetAdded>
    {
        private readonly ILogger<AssetAddedConsumer> _logger;
        private readonly IAssetsRepository _assetsRepository;

        public AssetAddedConsumer(ILogger<AssetAddedConsumer> logger,
            IAssetsRepository assetsRepository)
        {
            _logger = logger;
            _assetsRepository = assetsRepository;
        }

        public async Task Consume(ConsumeContext<AssetAdded> context)
        {
            var evt = context.Message;

            await _assetsRepository.AddOrReplaceAsync(
                new Asset
                {
                    Id = evt.AssetId,
                    BlockchainId = evt.BlockchainId,
                    Symbol = evt.Symbol,
                    Address = evt.Address,
                    Accuracy = evt.Accuracy
                }
            );

            await Task.CompletedTask;
        }
    }
}
