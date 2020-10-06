namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public enum WithdrawalState
    {
        Processing,

        Executing,

        Sent,

        Completed,

        Failed,

        Validating,

        Signing,
    }
}
