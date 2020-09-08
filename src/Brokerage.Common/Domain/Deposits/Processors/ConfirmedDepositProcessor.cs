using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Configuration;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedDepositProcessor : IConfirmedTransactionProcessor
    {
        private readonly IOperationsFactory _operationsFactory;
        private readonly IReadOnlyDictionary<string, BlockchainConfig> _blockchainsConfig;

        public ConfirmedDepositProcessor(IOperationsFactory operationsFactory, AppConfig appConfig)
        {
            _operationsFactory = operationsFactory;
            _blockchainsConfig = appConfig.Blockchains;
        }

        public async Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return;
            }

            //Always 0 for tag type blockchains
            var minDepositForConsolidation = 0m;
            if (processingContext.Blockchain.Protocol.Capabilities.DestinationTag == null &&
                _blockchainsConfig.TryGetValue(processingContext.Blockchain.Id, out var blockchainConfiguration))
            {
                minDepositForConsolidation = blockchainConfiguration.MinDepositForConsolidation;
            }

            var regularDeposits = processingContext.Deposits
                .Where(x => !x.IsBrokerDeposit)
                .ToArray();

            var normalDeposits = regularDeposits
                .Where(x => x.Unit.Amount >= minDepositForConsolidation);

            var groupedBalanceChanges = normalDeposits
                .GroupBy(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId));

            var balanceChanges = groupedBalanceChanges
                .Select(x => new
                {
                    Id = x.Key,
                    Amount = x.Sum(d => d.Unit.Amount),
                });

            if (processingContext.Blockchain.Protocol.Capabilities.DestinationTag != null)
            {
                foreach (var deposit in regularDeposits)
                {
                    deposit.ConfirmRegularWithDestinationTag(tx);
                }

                foreach (var change in balanceChanges)
                {
                    var balances = processingContext.BrokerAccountBalances[change.Id];

                    balances.ConfirmBrokerWithDestinationTagPendingBalance(change.Amount);
                }
            }
            else
            {
                var prevMinResiduals = processingContext.MinDepositResiduals
                    .ToLookup(x => x.AccountDetailsId);

                var lessThanMinRegularDeposits = regularDeposits
                    .Where(x => x.Unit.Amount < minDepositForConsolidation);

                //Added new residuals
                foreach (var minDeposit in lessThanMinRegularDeposits)
                {
                    var brokerAccountContext = processingContext.BrokerAccounts.Single(x => x.BrokerAccountId == minDeposit.BrokerAccountId);
                    var accountDetailsContext = brokerAccountContext.Accounts.Single(x => x.Details.Id == minDeposit.AccountDetailsId);
                    var accountDetails = accountDetailsContext.Details;

                    var minDepositResidual = await minDeposit.ConfirmMin(
                        accountDetails,
                        tx);

                    processingContext.AddNewMinDepositResidual(minDepositResidual);
                }

                foreach (var deposit in normalDeposits)
                {
                    var brokerAccountContext = processingContext.BrokerAccounts.Single(x => x.BrokerAccountId == deposit.BrokerAccountId);
                    var brokerAccountDetails = brokerAccountContext.AllBrokerAccountDetails[deposit.BrokerAccountDetailsId];
                    var accountDetailsContext = brokerAccountContext.Accounts.Single(x => x.Details.Id == deposit.AccountDetailsId);
                    var brokerAccount = brokerAccountContext.BrokerAccount;
                    var accountDetails = accountDetailsContext.Details;

                    var residuals = prevMinResiduals[accountDetails.NaturalId]
                        .Where(x => x.AssetId == deposit.Unit.AssetId)
                        .ToArray();

                    var margin = residuals.Sum(x => x.Amount);

                    var consolidationOperation = await deposit.ConfirmRegular(brokerAccount,
                        brokerAccountDetails,
                        accountDetails,
                        tx,
                        _operationsFactory, margin);

                    foreach (var residual in residuals)
                    {
                        residual.AddConsolidationDeposit(deposit.Id);
                    }

                    processingContext.AddNewOperation(consolidationOperation);
                }

                foreach (var change in balanceChanges)
                {
                    var balances = processingContext.BrokerAccountBalances[change.Id];

                    balances.ConfirmRegularPendingBalance(change.Amount);
                }
            }
        }
    }
}
