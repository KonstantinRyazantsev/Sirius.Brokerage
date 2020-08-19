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

            var regularDeposits = processingContext.Deposits
                .Where(x => !x.IsBrokerDeposit)
                .ToArray();

            var balanceChanges = regularDeposits
                .GroupBy(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                .Select(x => new
                {
                    Id = x.Key,
                    Amount = x.Sum(d => d.Unit.Amount)
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
                foreach (var deposit in regularDeposits)
                {
                    var brokerAccountContext = processingContext.BrokerAccounts.Single(x => x.BrokerAccountId == deposit.BrokerAccountId);
                    var brokerAccountDetails = brokerAccountContext.BrokerAccountDetails[deposit.BrokerAccountDetailsId];
                    var accountDetailsContext = brokerAccountContext.Accounts.Single(x => x.Details.AccountId == deposit.AccountDetailsId);
                    var brokerAccount = brokerAccountContext.BrokerAccount;
                    var accountDetails = accountDetailsContext.Details;

                    var consolidationOperation = await deposit.ConfirmRegular(brokerAccount,
                        brokerAccountDetails,
                        accountDetails,
                        tx,
                        _operationsFactory);

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
