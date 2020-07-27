namespace Brokerage.Common.Persistence
{
    public static class Tables
    {
        public const string BrokerAccounts = "broker_accounts";
        public const string BrokerAccountDetails = "broker_account_details";
        public const string BrokerAccountBalances = "broker_account_balances";
        public const string BrokerAccountBalancesUpdate = "broker_account_balances_update";

        public const string Accounts = "accounts";
        public const string AccountDetails = "account_details";

        public const string Blockchains = "blockchains";
        public const string Assets = "assets";

        public const string Deposits = "deposits";
        public const string DepositSources = "deposit_sources";
        public const string DepositFees = "deposit_fees";

        public const string Withdrawals = "withdrawals";
        public const string WithdrawalFees = "withdrawals_fees";
        
        public const string Operations = "operations";
        public const string ActualOperationFees = "actual_operation_fees";
        public const string ExpectedOperationFees = "expected_operation_fees";


        public const string DetectedTransactions = "detected_transactions";
    }
}
