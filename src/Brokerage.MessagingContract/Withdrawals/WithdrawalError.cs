namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class WithdrawalError
    {
        public string Message { get; set; }

        public WithdrawalErrorCode Code { get; set; }
    }
}
