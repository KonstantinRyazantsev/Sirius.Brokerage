﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.AccountRequisites)]
    public class AccountRequisitesEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string RequestId { get; set; }

        [ForeignKey(Tables.Accounts)]
        public long AccountId { get; set; }

        public AccountEntity Account { get; set; }

        public string BlockchainId { get; set; }
        public string Address { get; set; }
        public string Tag { get; set; }
        public TagTypeEnum? TagType { get; set; }
    }
}
