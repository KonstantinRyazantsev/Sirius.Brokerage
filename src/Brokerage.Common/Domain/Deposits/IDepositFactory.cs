using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits
{
    public interface IDepositFactory
    {
        Deposit Create(
            long id,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountDetailsId,
            Unit unit,
            TransactionInfo transactionInfo,
            IReadOnlyCollection<DepositSource> sources,
            decimal minDepositForConsolidation,
            DepositType depositType);

        Deposit Restore(
            long id,
            uint version,
            long sequence,
            string tenantId,
            string blockchainId,
            long brokerAccountId,
            long brokerAccountDetailsId,
            long? accountDetailsId,
            Unit unit,
            long? consolidationOperationId,
            IReadOnlyCollection<Unit> fees,
            TransactionInfo transactionInfo,
            DepositError error,
            DepositState depositState,
            IReadOnlyCollection<DepositSource> sources,
            DateTime createdAt,
            DateTime updatedAt,
            decimal minDepositForConsolidation,
            DepositType depositType);
    }
}
