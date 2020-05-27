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
        private readonly IBrokerAccountDetailsRepository _brokerAccountDetailsRepository;
        private readonly IAccountDetailsRepository _accountDetailsRepository;
        private readonly IOperationsExecutor _operationsExecutor;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;

        public ConfirmedDepositProcessor(IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IAccountDetailsRepository accountDetailsRepository,
            IOperationsExecutor operationsExecutor,
            IBrokerAccountsRepository brokerAccountsRepository)
        {
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
            _accountDetailsRepository = accountDetailsRepository;
            _operationsExecutor = operationsExecutor;
            _brokerAccountsRepository = brokerAccountsRepository;
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
                    _brokerAccountsRepository,
                    _brokerAccountDetailsRepository, 
                    _accountDetailsRepository, 
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
