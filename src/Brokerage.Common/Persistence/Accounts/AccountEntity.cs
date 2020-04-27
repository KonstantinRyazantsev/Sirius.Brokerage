using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Brokerage.Common.Persistence.BrokerAccount;

namespace Brokerage.Common.Persistence.Accounts
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
        public long Id { get; set; }

        [ForeignKey(Tables.BrokerAccounts)]
        public long BrokerAccountId { get; set; }

        public BrokerAccountEntity BrokerAccount { get; set; }

        public ICollection<AccountRequisitesEntity> AccountRequisites { get; set; }

        public string ReferenceId { get; set; }

        public AccountStateEnum State { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
