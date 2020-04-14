namespace Brokerage.Common.Domain.Withdrawals
{
    public class WithdrawalError
    {
        public WithdrawalError(string message, WithdrawalErrorCode code)
        {
            Message = message;
            Code = code;
        }

        public string Message { get; }

        public WithdrawalErrorCode Code { get; }

    }
}
