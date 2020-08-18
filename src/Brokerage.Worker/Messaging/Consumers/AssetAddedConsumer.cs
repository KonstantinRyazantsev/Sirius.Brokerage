using System.Threading.Tasks;
using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.ReadModels.Assets;
using MassTransit;
using Swisschain.Sirius.Indexer.MessagingContract;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class AssetAddedConsumer : IConsumer<AssetAdded>
    {
        private readonly IAssetsRepository _assetsRepository;

        public AssetAddedConsumer(IAssetsRepository assetsRepository)
        {
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
