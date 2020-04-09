namespace Brokerage.Common.Domain.Deposits
{
    public class DepositError
    {
        public DepositError(string message, DepositErrorCode code)
        {
            Message = message;
            Code = code;
        }
        public string Message { get; }

        public DepositErrorCode Code { get; }
    }
}
