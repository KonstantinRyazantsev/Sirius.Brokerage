using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.Transactions.Operations;
using Brokerage.Bilv1.Domain.Models.Transactions.Transfers;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.Domain.Services;

namespace Brokerage.Bilv1.DomainServices
{
    public class TransactionOrderService : ITransactionOrderService
    {
        private readonly IAssetService _assetService;
        private readonly IOperationRepository _operationRepository;

        public TransactionOrderService(IAssetService assetService, IOperationRepository operationRepository)
        {
            _assetService = assetService;
            _operationRepository = operationRepository;
        }

        public async Task<IReadOnlyCollection<TransactionOrder>> GetAllAsync(string blockchainId, string networkId, int skip, int take)
        {
            var operations = await _operationRepository.GetAllForBlockchainAsync(blockchainId, skip, take);
            return operations.Select(x =>
            {
                var asset = _assetService.GetAssetForId(blockchainId, networkId, x.Key.BlockchainAssetId);
                var amount = ConverterExtensions.ConvertFromString(x.BalanceChange.ToString(), asset.Accuracy, asset.Accuracy);

                return new TransactionOrder
                {
                    ConfirmedAtBlock = x.Block,
                    Sources = new List<TransferSource>
                    {
                        new TransferSource
                        {
                            Address = x.Key.WalletAddress,
                            Units = new List<TransferUnit>
                            {
                                new TransferUnit
                                {
                                    Amount = amount, AssetId = x.Key.BlockchainAssetId
                                }
                            }
                        }
                    },
                };
            }).ToArray();
        }
    }
}
