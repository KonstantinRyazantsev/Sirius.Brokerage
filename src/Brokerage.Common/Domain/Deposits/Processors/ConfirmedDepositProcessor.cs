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

        public ConfirmedDepositProcessor(IOperationsFactory operationsFactory)
        {
            _operationsFactory = operationsFactory;
        }

        public async Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return;
            }

            var regularDeposits = processingContext.Deposits
                .Where(x => !x.IsBrokerDeposit)
                .ToArray();

            var normalDeposits = regularDeposits
                .Where(x => !x.IsTiny);

            var groupedBalanceChanges = regularDeposits
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

                var tinyDeposits = regularDeposits
                    .Where(x => x.IsTiny);

                await ProcessTinyDeposits(tx, processingContext, tinyDeposits);

                var accDict = processingContext.Accounts.ToDictionary(x => x.Id);

                foreach (var deposit in normalDeposits)
                {
                    var brokerAccountContext = processingContext.BrokerAccounts.Single(x => x.BrokerAccountId == deposit.BrokerAccountId);
                    var brokerAccountDetails = brokerAccountContext.AllBrokerAccountDetails[deposit.BrokerAccountDetailsId];
                    var accountDetailsContext = brokerAccountContext.Accounts.Single(x => x.Details.Id == deposit.AccountDetailsId);
                    var brokerAccount = brokerAccountContext.BrokerAccount;
                    var accountDetails = accountDetailsContext.Details;
                    accDict.TryGetValue(accountDetails.AccountId, out var account);
                    var residuals = prevMinResiduals[accountDetails.NaturalId]
                        .Where(x => x.AssetId == deposit.Unit.AssetId)
                        .ToArray();

                    var margin = residuals.Sum(x => x.Amount);

                    var consolidationOperation = await deposit.ConfirmRegular(brokerAccount,
                        brokerAccountDetails,
                        accountDetails,
                        tx,
                        _operationsFactory, 
                        margin,
                        account);

                    foreach (var residual in residuals)
                    {
                        residual.AddConsolidationDeposit(deposit.Id);
                    }

                    processingContext.AddNewOperation(consolidationOperation);
                }


                //Moves pending to Owned
                foreach (var change in balanceChanges)
                {
                    var balances = processingContext.BrokerAccountBalances[change.Id];

                    balances.ConfirmRegularPendingBalance(change.Amount);
                }
            }
        }

        private static async Task ProcessTinyDeposits(TransactionConfirmed tx, TransactionProcessingContext processingContext, IEnumerable<Deposit> tinyDeposits)
        {
            foreach (var tinyDeposit in tinyDeposits)
            {
                var brokerAccountContext = processingContext.BrokerAccounts.Single(x => x.BrokerAccountId == tinyDeposit.BrokerAccountId);
                var accountDetailsContext = brokerAccountContext.Accounts.Single(x => x.Details.Id == tinyDeposit.AccountDetailsId);
                var accountDetails = accountDetailsContext.Details;

                var minDepositResidual = await tinyDeposit.ConfirmTiny(
                    accountDetails,
                    tx);

                processingContext.AddNewMinDepositResidual(minDepositResidual);
            }
        }
    }
}
