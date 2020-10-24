using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccounts;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Persistence.Transactions;
using Brokerage.Common.Persistence.Withdrawals;
using Swisschain.Extensions.Idempotency.EfCore;

namespace Brokerage.Common.Persistence
{
    public class UnitOfWork : UnitOfWorkBase<DatabaseContext>
    {
        public IAccountDetailsRepository AccountDetails { get; private set; }
        public IAccountsRepository Accounts { get; private set; }
        public IBrokerAccountDetailsRepository BrokerAccountDetails { get; private set; }
        public IBrokerAccountsBalancesRepository BrokerAccountBalances { get; private set; }
        public IBrokerAccountsRepository BrokerAccounts { get; private set; }
        public IDepositsRepository Deposits { get; private set; }
        public IWithdrawalsRepository Withdrawals { get; private set; }
        public IOperationsRepository Operations { get; private set; }
        public IDetectedTransactionsRepository DetectedTransactions { get; private set; }

        public IMinDepositResidualsRepository MinDepositResiduals { get; private set; }

        public IDepositFactory DepositFactory { get; private set; }

        protected override void ProvisionRepositories(DatabaseContext dbContext)
        {
            DepositFactory = new DepositFactory();
            AccountDetails = new AccountDetailsRepository(dbContext);
            Accounts = new AccountsRepository(dbContext);
            BrokerAccountDetails = new BrokerAccountDetailsRepository(dbContext);
            BrokerAccountBalances = new BrokerAccountsBalancesRepository(dbContext);
            BrokerAccounts = new BrokerAccountsRepository(dbContext);
            Deposits = new DepositsRepository(dbContext, DepositFactory);
            Withdrawals = new WithdrawalsRepository(dbContext);
            Operations = new OperationsRepository(dbContext);
            DetectedTransactions = new DetectedTransactionsRepository(dbContext);
            MinDepositResiduals = new MinDepositResidualsRepository(dbContext);
        }
    }
}
