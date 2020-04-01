using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.BrokerAccountBalancesUpdate)]
    public class BrokerAccountBalancesUpdateEntity
    {
        //string updateId(unique) (broker account balances ID + transaction ID)

        [Key]
        public string UpdateId { get; set; }
    }
}
