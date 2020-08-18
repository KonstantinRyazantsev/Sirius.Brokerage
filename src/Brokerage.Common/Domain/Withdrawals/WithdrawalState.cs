namespace Brokerage.Common.Domain.Withdrawals
{
    public enum WithdrawalState
    {
        Processing,
        Executing,
        Sent,
        Completed,
        Failed
    }
}
