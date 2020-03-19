using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{

    [Table(name: Tables.BlockchainsTableName)]
    public class BlockchainEntity
    {
        [Key]
        public string BlockchainId { get; set; }
    }
}
