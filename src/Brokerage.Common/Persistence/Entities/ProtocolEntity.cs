using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brokerage.Common.Persistence.Entities
{
    [Table(name: Tables.ProtocolsTableName)]
    public class ProtocolEntity
    {
        [Key]
        public string ProtocolId { get; set; }
    }
}
