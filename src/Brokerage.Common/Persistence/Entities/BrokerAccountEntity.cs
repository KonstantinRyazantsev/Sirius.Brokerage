using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.BrokerAccountsTableName)]
    public class BrokerAccountEntity
    {
        public string RequestId { get; set; }

        public string TenantId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BrokerAccountId { get; set; }

        public string Name { get; set; }

        public BrokerAccountStateEnum State { get; set; }

        public DateTime CreationDateTime { get; set; }

        public DateTime? ActivationDateTime { get; set; }

        public DateTime? BlockingDateTime { get; set; }
    }
}
