using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Brokerage.Common.Persistence.Accounts;

namespace Brokerage.Common.Persistence.BrokerAccounts
{
    [Table(name: Tables.BrokerAccounts)]
    public class BrokerAccountEntity
    {
        public BrokerAccountEntity()
        {
            Accounts = new HashSet<AccountEntity>();
        }

        public string TenantId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }

        public BrokerAccountStateEnum State { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public long VaultId { get; set; }

        public ICollection<AccountEntity> Accounts { get; set; }
    }
}
