using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.BrokerAccount
{
    [Table(name: Tables.BrokerAccountDetails)]
    public class BrokerAccountDetailsEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string NaturalId { get; set; }
        public string ActiveId { get; set; }
        public string BlockchainId { get; set; }
        public string TenantId { get; set; }
        public long BrokerAccountId { get; set; }
        public string Address { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
