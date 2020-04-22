using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Executor.ApiClient;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedDepositProcessor : IConfirmedTransactionProcessor
    {
        private readonly IExecutorClient _executorClient;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;

        public ConfirmedDepositProcessor(IExecutorClient executorClient,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository)
        {
            _executorClient = executorClient;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _accountRequisitesRepository = accountRequisitesRepository;
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
                    _executorClient);
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
