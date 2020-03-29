using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.Accounts)]
    public class AccountEntity
    {
        public AccountEntity()
        {
            AccountRequisites = new HashSet<AccountRequisitesEntity>();
        }

        public string RequestId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountId { get; set; }

        [ForeignKey(Tables.BrokerAccounts)]
        public long BrokerAccountId { get; set; }

        public BrokerAccountEntity BrokerAccount { get; set; }

        public ICollection<AccountRequisitesEntity> AccountRequisites { get; set; }

        public string ReferenceId { get; set; }

        public AccountStateEnum State { get; set; }

        public DateTimeOffset CreationDateTime { get; set; }

        public DateTimeOffset? ActivationDateTime { get; set; }

        public DateTimeOffset? BlockingDateTime { get; set; }
    }
}
