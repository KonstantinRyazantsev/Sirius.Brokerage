using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.BrokerAccountRequisites)]
    public class BrokerAccountRequisitesEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string RequestId { get; set; }
        public string BlockchainId { get; set; }
        public long BrokerAccountId { get; set; }
        public string Address { get; set; }
        public DateTimeOffset CreationDateTime { get; set; }
    }
}
