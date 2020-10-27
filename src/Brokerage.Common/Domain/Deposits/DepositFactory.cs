using System;
using System.Collections.Generic;
using Brokerage.Common.Domain.Deposits.Implementations;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Deposits
{
    public class DepositFactory : IDepositFactory
    {
        public DepositFactory()
        {
        }

        public Deposit Restore(
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
            DepositType depositType)
        {
            Deposit deposit;

            switch (depositType)
            {
                case DepositType.Broker:
                    deposit = BrokerDeposit.Restore(
                        id,
                        version,
                        sequence,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        consolidationOperationId,
                        fees,
                        transactionInfo,
                        error,
                        depositState,
                        sources,
                        createdAt,
                        updatedAt,
                        minDepositForConsolidation);
                    break;
                case DepositType.Regular:
                    deposit = RegularDeposit.Restore(
                        id,
                        version,
                        sequence,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        consolidationOperationId,
                        fees,
                        transactionInfo,
                        error,
                        depositState,
                        sources,
                        createdAt,
                        updatedAt,
                        minDepositForConsolidation);
                    break;
                case DepositType.Tiny:
                    deposit = TinyDeposit.Restore(
                        id,
                        version,
                        sequence,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        consolidationOperationId,
                        fees,
                        transactionInfo,
                        error,
                        depositState,
                        sources,
                        createdAt,
                        updatedAt,
                        minDepositForConsolidation);
                    break;
                case DepositType.Token:
                    deposit = TokenDeposit.Restore(
                        id,
                        version,
                        sequence,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        consolidationOperationId,
                        fees,
                        transactionInfo,
                        error,
                        depositState,
                        sources,
                        createdAt,
                        updatedAt,
                        minDepositForConsolidation);
                    break;
                case DepositType.TinyToken:
                    deposit = TinyTokenDeposit.Restore(
                        id,
                        version,
                        sequence,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        consolidationOperationId,
                        fees,
                        transactionInfo,
                        error,
                        depositState,
                        sources,
                        createdAt,
                        updatedAt,
                        minDepositForConsolidation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(depositType), depositType, null);
            }

            return deposit;
        }
    }
}
