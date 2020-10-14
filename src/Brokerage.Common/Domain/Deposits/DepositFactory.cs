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

        public Deposit Create(
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
            DepositType depositType)
        {
            Deposit deposit;

            switch (depositType)
            {
                case DepositType.BrokerDeposit:
                    deposit = BrokerDeposit.Create(
                        id,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        transactionInfo,
                        sources,
                        minDepositForConsolidation);
                    break;
                case DepositType.RegularDeposit:
                    deposit = RegularDeposit.Create(
                        id,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        transactionInfo,
                        sources,
                        minDepositForConsolidation);
                    break;
                case DepositType.TinyDeposit:
                    deposit = TinyDeposit.Create(
                        id,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        transactionInfo,
                        sources,
                        minDepositForConsolidation);
                    break;
                case DepositType.TokenDeposit:
                    deposit = TokenDeposit.Create(
                        id,
                        tenantId,
                        blockchainId,
                        brokerAccountId,
                        brokerAccountDetailsId,
                        accountDetailsId,
                        unit,
                        transactionInfo,
                        sources,
                        minDepositForConsolidation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(depositType), depositType, null);
            }

            return deposit;
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
                case DepositType.BrokerDeposit:
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
                case DepositType.RegularDeposit:
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
                case DepositType.TinyDeposit:
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
                case DepositType.TokenDeposit:
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(depositType), depositType, null);
            }

            return deposit;
        }
    }
}
