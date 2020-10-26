using System.Threading.Tasks;
using Brokerage.Common.Domain.Withdrawals;
using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;

namespace Brokerage.Common.Domain.Operations
{
    public interface IOperationsFactory
    {
        Task<Operation> StartDepositConsolidation(string tenantId,
            long depositId,
            string accountAddress,
            string brokerAccountAddress,
            Unit unit,
            long asAtBlockNumber,
            long vaultId,
            string accountReferenceId,
            long brokerAccountId);

        Task<Operation> StartDepositProvisioning(string tenantId,
            long depositId,
            string accountAddress,
            string brokerAccountAddress,
            Unit unit,
            long asAtBlockNumber,
            long vaultId,
            string accountReferenceId,
            long brokerAccountId);

        Task<Operation> StartWithdrawal(string tenantId,
            long withdrawalId,
            string brokerAccountAddress,
            DestinationDetails destinationDetails,
            Unit unit,
            long vaultId,
            TransferContext transferContext,
            string sourceGroup,
            string destinationGroup);
    }
}
