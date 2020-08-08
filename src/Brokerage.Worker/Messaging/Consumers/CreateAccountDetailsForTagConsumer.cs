using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class CreateAccountDetailsForTagConsumer : IConsumer<CreateAccountDetailsForTag>
    {
        private readonly ILogger<FinalizeAccountCreationConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IAccountDetailsRepository _accountDetailsRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOutboxManager _outboxManager;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;
        private readonly IBrokerAccountDetailsRepository _brokerAccountDetailsRepository;

        public CreateAccountDetailsForTagConsumer(
            ILogger<FinalizeAccountCreationConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IAccountDetailsRepository accountDetailsRepository,
            IAccountsRepository accountsRepository,
            IOutboxManager outboxManager,
            IBrokerAccountsRepository brokerAccountsRepository,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository)
        {
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _accountDetailsRepository = accountDetailsRepository;
            _accountsRepository = accountsRepository;
            _outboxManager = outboxManager;
            _brokerAccountsRepository = brokerAccountsRepository;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
        }

        public async Task Consume(ConsumeContext<CreateAccountDetailsForTag> context)
        {
            var command = context.Message;
            var account = await _accountsRepository.GetAsync(command.AccountId);
            var brokerAccount = await _brokerAccountsRepository.GetAsync(account.BrokerAccountId);
            var blockchain = await _blockchainsRepository.GetAsync(command.BlockchainId);
            var destinationTagType = DestinationTagType.Number;
            var tagGenerator = _destinationTagGeneratorFactory.Create(blockchain);

            var tag = tagGenerator.Generate();
            //TODO: It fails sometimes when there is no active br
            var brokerAccountDetails = await _brokerAccountDetailsRepository
                .GetActiveAsync(new ActiveBrokerAccountDetailsId(blockchain.Id, brokerAccount.Id));

            var outbox = await _outboxManager.Open($"AccountDetails:Create:{account.Id}:{blockchain.Id}",
                () => _accountDetailsRepository.GetNextIdAsync());
            if (!outbox.IsStored)
            {
                var accountDetails = AccountDetails.Create(
                    outbox.AggregateId,
                    new AccountDetailsId(blockchain.Id, brokerAccountDetails.NaturalId.Address, tag, destinationTagType),
                    account.Id,
                    brokerAccount.Id);

                await account.AddAccountDetails(
                    _accountDetailsRepository,
                    _accountsRepository,
                    accountDetails,
                    command.ExpectedCount);

                foreach (var item in account.Events)
                {
                    outbox.Publish(item);
                }

                await _outboxManager.Store(outbox);
            }

            await _outboxManager.EnsureDispatched(outbox);
        }
    }
}
