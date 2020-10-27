using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits.Implementations;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Domain.Processing;
using Brokerage.Common.Domain.Processing.Context;
using Swisschain.Sirius.Confirmator.MessagingContract;

namespace Brokerage.Common.Domain.Deposits.Processors
{
    public class ConfirmedTinyDepositProcessor : IConfirmedTransactionProcessor
    {
        private readonly IOperationsFactory _operationsFactory;

        public ConfirmedTinyDepositProcessor(IOperationsFactory operationsFactory)
        {
            _operationsFactory = operationsFactory;
        }

        public Task Process(TransactionConfirmed tx, TransactionProcessingContext processingContext)
        {
            if (processingContext.Operation?.Type == OperationType.DepositProvisioning ||
                processingContext.Operation?.Type == OperationType.DepositConsolidation)
            {
                return Task.CompletedTask;
            }

            if (processingContext.Blockchain.Protocol.Capabilities.DestinationTag == null)
            {
                var tinyDeposits = processingContext.TinyDeposits;

                ProcessTinyDeposits(tx, processingContext, tinyDeposits);
            }

            return Task.CompletedTask;
        }

        private static void ProcessTinyDeposits(TransactionConfirmed tx,
            TransactionProcessingContext processingContext,
            IEnumerable<TinyDeposit> tinyDeposits)
        {
            foreach (var tinyDeposit in tinyDeposits)
            {
                var brokerAccountContext = processingContext.BrokerAccounts.Single(x => x.BrokerAccountId == tinyDeposit.BrokerAccountId);
                var accountDetailsContext = brokerAccountContext.Accounts.Single(x => x.Details.Id == tinyDeposit.AccountDetailsId);
                var accountDetails = accountDetailsContext.Details;

                tinyDeposit.Confirm(tx);
                var minDepositResidual = tinyDeposit.GetResidual(accountDetails);
                processingContext.AddNewMinDepositResidual(minDepositResidual);
            }
        }
    }
}
