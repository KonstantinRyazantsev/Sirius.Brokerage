using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.BrokerAccounts)]
    public class BrokerAccountEntity
    {
        public BrokerAccountEntity()
        {
            Accounts = new HashSet<AccountEntity>();
        }
        public string RequestId { get; set; }

        public string TenantId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BrokerAccountId { get; set; }

        public string Name { get; set; }

        public BrokerAccountStateEnum State { get; set; }

        public DateTimeOffset CreationDateTime { get; set; }

        public DateTimeOffset? ActivationDateTime { get; set; }

        public DateTimeOffset? BlockingDateTime { get; set; }

        public ICollection<AccountEntity> Accounts { get; set; }

        public BrokerAccountBalancesEntity BrokerAccountBalances { get; set; }
    }
}
