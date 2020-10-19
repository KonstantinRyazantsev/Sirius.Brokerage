namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class TransferContext
    {
        public string AccountReferenceId { get; set; }

        public string WithdrawalReferenceId { get; set; }

        public string Document { get; set; }

        public string Signature { get; set; }

        public RequestContext RequestContext { get; set; }
    }
}
