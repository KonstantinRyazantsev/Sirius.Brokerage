using System;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Services;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract.Assets;

namespace Brokerage.Bilv1.DomainServices
{
    public class TransferService : ITransferService
    {
        private readonly IAssetService _assetService;
        private readonly BlockchainApiClientProvider _blockchainApiClientProvider;

        public TransferService(IAssetService assetService,
            BlockchainApiClientProvider blockchainApiClientProvider)
        {
            _assetService = assetService;
            _blockchainApiClientProvider = blockchainApiClientProvider;
        }

        public async Task<string> BuildTransactionAsync(
            Guid requestId,
            string blockchainId,
            string blockchainAssetId,
            string networkId,
            string fromAddress,
            string toAddress,
            decimal amount,
            string pubKey = null)
        {
            var asset = _assetService.GetAssetForId(blockchainId, networkId, blockchainAssetId);
            var strAmount = ConverterExtensions.ConvertToString(amount, asset.Accuracy, asset.Accuracy);

            var blockchainApiClient = _blockchainApiClientProvider.Get(blockchainId);
            var amountDecimal = ConverterExtensions.ConvertFromString(strAmount, asset.Accuracy, asset.Accuracy);
            var builtTransaction = await blockchainApiClient.BuildSingleTransactionAsync
            (
                requestId,
                fromAddress,
                pubKey == "" ? null : "{\"PubKey\":\"" + pubKey + "\"}",
                toAddress,
                new BlockchainAsset(new AssetContract
                {
                    Accuracy = asset.Accuracy,
                    Address = asset.Address,
                    AssetId = asset.AssetId,
                    Name = asset.Ticker
                }),
                amountDecimal,
                includeFee: true
            );
            
            return builtTransaction.TransactionContext;
        }

        public async Task BroadcastTransactionAsync(
            Guid requestId,
            string networkId,
            string blockchainId,
            string signedTransaction
        )
        {
            var blockchainApiClient = _blockchainApiClientProvider.Get(blockchainId);
            var result = await blockchainApiClient.BroadcastTransactionAsync(requestId, signedTransaction);

            if (result != TransactionBroadcastingResult.Success)
            {
                throw new Exception($"Broadcasting failed: {result}");
            }
        }
    }
}
