using Brokerage.Common.Domain.Operations;

namespace Brokerage.Common.Persistence.Operations
{
    public class OperationEntity
    {
        public long Id { get; set; }
        public OperationType Type { get; set; }
    }
}
