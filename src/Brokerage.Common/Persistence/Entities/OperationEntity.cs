using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Operations;

namespace Brokerage.Common.Persistence.Entities
{
    public class OperationEntity
    {
        public long Id { get; set; }
        public OperationType Type { get; set; }
    }
}
