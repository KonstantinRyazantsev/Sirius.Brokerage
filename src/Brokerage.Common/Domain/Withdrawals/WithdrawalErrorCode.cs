namespace Brokerage.Common.Domain.Withdrawals
{
    public enum WithdrawalErrorCode
    {
        NotEnoughBalance,
        InvalidDestinationAddress,
        DestinationTagRequired,
        TechnicalProblem,
        ValidationRejected
    }
}
