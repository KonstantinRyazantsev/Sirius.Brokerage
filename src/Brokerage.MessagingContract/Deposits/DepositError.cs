namespace Swisschain.Sirius.Brokerage.MessagingContract.Deposits
{
    public class DepositError
    {
        public string Message { get; set; }

        public DepositErrorCode Code { get; set; }

        public enum DepositErrorCode
        {
            TechnicalProblem
        }
    }
}
