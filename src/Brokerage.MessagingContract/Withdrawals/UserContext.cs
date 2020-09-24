﻿namespace Swisschain.Sirius.Brokerage.MessagingContract.Withdrawals
{
    public class UserContext
    {
        public string PassClientIp { get; set; }
        public string UserId { get; set; }
        public string ApiKeyId { get; set; }
        public string AccountReferenceId { get; set; }
        public string WithdrawalReferenceId { get; set; }
    }
}
