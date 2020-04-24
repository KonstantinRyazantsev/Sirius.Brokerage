namespace Brokerage.Common.Domain
{
    public enum BalanceChangingReason
    {
        TransactionDetected,
        TransactionConfirmed,
        OperationStarted,
        OperationSent,
        OperationCompleted,
        OperationFailed
    }
}
