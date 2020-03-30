namespace Brokerage.Bilv1.Domain.Models.Transactions.Operations
{
    public enum OperationState
    {
        Broadcasting,
        Broadcasted,
        Accepted,
        Confirmed,
        Failed
    }
}
