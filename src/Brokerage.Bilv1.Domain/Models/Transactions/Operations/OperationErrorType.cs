namespace Brokerage.Bilv1.Domain.Models.Transactions.Operations
{
    public enum OperationErrorType
    {
        Unknown,
        NotEnoughBalance,
        FeeTooLow,
        RebuildRequired
    }
}
