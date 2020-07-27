using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Deposits
{
    public class DepositFeeEntity
    {
        public long DepositId { get; set; }

        public DepositEntity DepositEntity { get; set; }

        public long AssetId { get; set; }

        public decimal Amount { get; set; }
    }
}
