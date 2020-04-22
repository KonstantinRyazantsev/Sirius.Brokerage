using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;
using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;

namespace Brokerage.Common.Domain.Operations
{
    public interface IOperationsExecutor
    {
        Task<Operation> StartDepositConsolidation(string tenantId,
            long depositId,
            string accountAddress,
            string brokerAccountAddress,
            Unit unit,
            long asAtBlockNumber);

        Task<Operation> StartWithdrawal(string tenantId,
            long withdrawalId,
            string brokerAccountAddress,
            DestinationRequisites destinationRequisites,
            Unit unit);
    }
}
