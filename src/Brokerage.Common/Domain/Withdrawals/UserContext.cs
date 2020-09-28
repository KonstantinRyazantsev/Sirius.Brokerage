using System;
using System.Collections.Generic;
using System.Text;

namespace Brokerage.Common.Domain.Withdrawals
{
    public class UserContext
    {
        public string PassClientIp { get; set; }
        public string UserId { get; set; }
        public string ApiKeyId { get; set; }
        public string AccountReferenceId { get; set; }
        public string WithdrawalReferenceId { get; set; }

        public string WithdrawalParamsSignature { get; set; }
    }
}
