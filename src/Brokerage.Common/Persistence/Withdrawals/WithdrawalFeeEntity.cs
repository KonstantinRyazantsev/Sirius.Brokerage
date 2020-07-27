using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Withdrawals
{
    public class WithdrawalFeeEntity
    {
        public long WithdrawalId { get; set; }

        public WithdrawalEntity WithdrawalEntity { get; set; }

        public long AssetId { get; set; }

        public decimal Amount { get; set; }
    }
}
