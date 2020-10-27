using System.Linq;
using System.Threading.Tasks;
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

            var regularDeposits = processingContext.RegularDeposits
                .ToArray();

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
                    deposit.CompleteWithDestinationTag(tx);
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

                var accDict = processingContext.Accounts.ToDictionary(x => x.Id);

                foreach (var deposit in regularDeposits)
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

                    var consolidationOperation = await deposit.Confirm(brokerAccount,
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
    }
}
