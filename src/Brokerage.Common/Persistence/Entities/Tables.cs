namespace Brokerage.Common.Persistence.Entities
{
    public static class Tables
    {
        public const string BrokerAccounts = "broker_accounts";
        public const string BrokerAccountRequisites = "broker_account_requisites";
        public const string BrokerAccountBalances = "broker_account_balances";
        public const string BrokerAccountBalancesUpdate = "broker_account_balances_update";

        public const string Accounts = "accounts";
        public const string AccountRequisites = "account_requisites";

        public const string Blockchains = "blockchains";

        public const string Deposits = "deposits";
        public const string DepositSources = "deposit_sources";
        public const string DepositFees = "deposit_fees";

        public const string Withdrawals = "withdrawals";
        public const string WithdrawalFees = "withdrawals_fees";

        public const string Assets = "assets";
    }
}
