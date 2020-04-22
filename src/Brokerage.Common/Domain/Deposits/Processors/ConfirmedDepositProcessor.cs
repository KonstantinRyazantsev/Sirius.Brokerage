using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedDepositProcessor : IConfirmedTransactionProcessor
    {
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IOperationsExecutor _operationsExecutor;

        public ConfirmedDepositProcessor(IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository,
            IOperationsExecutor operationsExecutor)
        {
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _accountRequisitesRepository = accountRequisitesRepository;
            _operationsExecutor = operationsExecutor;
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

            foreach (var deposit in regularDeposits)
            {
                await deposit.ConfirmRegular(
                    _brokerAccountRequisitesRepository, 
                    _accountRequisitesRepository, 
                    tx,
                    _operationsExecutor);
            }

            var balanceChanges = regularDeposits
                .GroupBy(x => new BrokerAccountBalancesId(x.BrokerAccountId, x.Unit.AssetId))
                .Select(x => new
                {
                    Id = x.Key,
                    Amount = x.Sum(d => d.Unit.Amount)
                });

            foreach (var change in balanceChanges)
            {
                var balances = processingContext.BrokerAccountBalances[change.Id];

                balances.ConfirmRegularPendingBalance(change.Amount);
            }
        }
    }
}
