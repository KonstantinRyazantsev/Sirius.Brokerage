using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Deposits
{
    public class DepositSourceEntity
    {
        public long DepositId { get; set; }

        public DepositEntity DepositEntity { get; set; }

        public string Address { get; set; }

        public decimal Amount { get; set; }
    }
}
