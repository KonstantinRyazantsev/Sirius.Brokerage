using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.NetworksTableName)]
    public class NetworkEntity
    {
        [Key, Column(Order = 0)]
        public string BlockchainId { get; set; }

        [Key, Column(Order = 0)]
        public string NetworkId { get; set; }
    }
}
