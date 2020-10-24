using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Executor.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class CompletedDepositProcessor : ICompletedOperationProcessor
    {
        public Task Process(OperationCompleted evt, OperationProcessingContext processingContext)
        {
            if (processingContext.Operation.Type != OperationType.DepositConsolidation)
            {
                return Task.CompletedTask;
            }

            var minDepositDictionary = processingContext.MinDeposits.ToDictionary(x => x.Id);
            var minDepositLookup = processingContext.MinDepositResiduals
                .Where(x => x.ConsolidationDepositId.HasValue)
                .ToLookup(x => x.ConsolidationDepositId.Value, y => minDepositDictionary[y.DepositId]);

            foreach (var deposit in processingContext.RegularDeposits)
            {
                var assetId = deposit.Unit.AssetId;
                var minDeposits = minDepositLookup[deposit.Id]
                    .Where(x => x.Unit.AssetId == assetId)
                    .ToArray();
                var distributedFee = evt.ActualFees;

                if (minDeposits.Any())
                {
                    var count = minDeposits.Count() + 1;
                    var mainFeeUnit = distributedFee.FirstOrDefault(x => x.AssetId == assetId) ?? new Unit(assetId, 0m);
                    var listFees = new List<Unit>(4);
                    listFees.AddRange(distributedFee
                            .Where(x => x.AssetId != assetId));
                    listFees.Add(new Unit(assetId, mainFeeUnit.Amount / count));
                    distributedFee = listFees;
                }

                deposit.Complete(distributedFee);

                foreach (var minDeposit in minDeposits)
                {
                    minDeposit.Complete(distributedFee);
                }
            }

            return Task.CompletedTask;
        }
    }
}
