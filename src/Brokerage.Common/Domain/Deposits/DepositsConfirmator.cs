using System.Threading.Tasks;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using MassTransit;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Executor.ApiClient;
using Swisschain.Sirius.Executor.ApiContract.Common;
using Swisschain.Sirius.Executor.ApiContract.Transfers;

namespace Brokerage.Common.Domain.Deposits
{
    public class DepositsConfirmator
    {
        private readonly IDepositsRepository _depositsRepository;
        private readonly IExecutorClient _executorClient;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public DepositsConfirmator(
            IDepositsRepository depositsRepository,
            Swisschain.Sirius.Executor.ApiClient.IExecutorClient executorClient,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountsRepository brokerAccountsRepository,
            IPublishEndpoint publishEndpoint)
        {
            _depositsRepository = depositsRepository;
            _executorClient = executorClient;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountsRepository = brokerAccountsRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Confirm(TransactionConfirmed transaction)
        {
            var deposits = await _depositsRepository.GetByTransactionIdAsync(transaction.TransactionId);

            foreach (var deposit in deposits)
            {
                deposit.Confirm();

                if (!deposit.IsBrokerDeposit)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    var accountRequisites = await _accountRequisitesRepository.GetByIdAsync(deposit.AccountRequisitesId.Value);
                    var brokerAccountRequisites = await _brokerAccountRequisitesRepository.GetByIdAsync(deposit.BrokerAccountRequisitesId);
                    var brokerAccount = await _brokerAccountsRepository.GetAsync(brokerAccountRequisites.BrokerAccountId);

                    await _executorClient.Transfers.ExecuteAsync(
                        new ExecuteTransferRequest(new ExecuteTransferRequest()
                        {
                            AssetId = deposit.AssetId,
                            Operation = new OperationRequest()
                            {
                                //TODO: Specify as at block number
                                RequestId = $"{deposit.Id}",
                                FeePayerAddress = brokerAccountRequisites.Address,
                                TenantId = brokerAccount.TenantId,
                            },
                            Movements =
                            {
                                new Movement()
                                {
                                    SourceAddress = accountRequisites.Address,
                                    DestinationAddress = brokerAccountRequisites.Address,
                                    Amount = deposit.Amount,
                                }
                            }
                        }));
                }

                await _depositsRepository.SaveAsync(deposit);

                foreach (var evt in deposit.Events)
                {
                    await _publishEndpoint.Publish(evt);
                }
            }
        }
    }
}
