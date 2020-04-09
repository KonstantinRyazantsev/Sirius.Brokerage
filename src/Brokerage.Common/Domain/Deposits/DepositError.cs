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

        public enum DepositErrorCode
        {
            TechnicalProblem = 0,
            NotEnoughBalance = 1,
            InvalidDestinationAddress = 2,
            DestinationTagRequired = 3,
            AmountIsTooSmall = 4
        }
    }
}
