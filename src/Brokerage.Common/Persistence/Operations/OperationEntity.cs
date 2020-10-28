using System.Collections.Generic;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Persistence.Withdrawals;

namespace Brokerage.Common.Persistence.Operations
{
    public class OperationEntity
    {
        public OperationEntity()
        {
            ExpectedFees = new HashSet<ExpectedOperationFeeEntity>();
            ActualFees = new HashSet<ActualOperationFeeEntity>();
        }

        public IReadOnlyCollection<ActualOperationFeeEntity> ActualFees { get; set; }

        public IReadOnlyCollection<ExpectedOperationFeeEntity> ExpectedFees { get; set; }

        public long Id { get; set; }
        public string BlockchainId { get; set; }
        public OperationType Type { get; set; }

        public uint Version { get; set; }

    }
}
