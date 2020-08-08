using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.VaultAgent.MessagingContract.Wallets;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class WalletAddedConsumer : IConsumer<WalletAdded>
    {
        private readonly ILogger<WalletAddedConsumer> _logger;
        private readonly IBrokerAccountsRepository _brokerAccountsRepository;
        private readonly IBrokerAccountDetailsRepository _brokerAccountDetailsRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IAccountDetailsRepository _accountDetailsRepository;
        private readonly IOutboxManager _outboxManager;

        public WalletAddedConsumer(ILogger<WalletAddedConsumer> logger,
            IBrokerAccountsRepository brokerAccountsRepository,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IAccountsRepository accountsRepository,
            IAccountDetailsRepository accountDetailsRepository,
            IOutboxManager outboxManager)
        {
            _logger = logger;
            _brokerAccountsRepository = brokerAccountsRepository;
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
            _accountsRepository = accountsRepository;
            _accountDetailsRepository = accountDetailsRepository;
            _outboxManager = outboxManager;
        }

        public async Task Consume(ConsumeContext<WalletAdded> context)
        {
            var evt = context.Message;

            if (evt.Component != nameof(Brokerage))
            {
                _logger.LogInformation("WalletAdded has been skipped due to component value {@context}", evt);

                return;
            }

            if (string.IsNullOrEmpty(evt.Context))
            {
                _logger.LogError("WalletAdded has been skipped due to context value {@context}", evt);

                throw new ArgumentException("Context is empty", nameof(evt.Context));
            }

            var requesterContext = Newtonsoft.Json.JsonConvert.DeserializeObject<WalletGenerationRequesterContext>(evt.Context);

            switch (requesterContext.AggregateType)
            {
                case AggregateType.Account:
                    {
                        var outbox = await _outboxManager.Open($"AccountDetails:Create:{evt.WalletGenerationRequestId}",
                            () => _accountDetailsRepository.GetNextIdAsync());
                        if (!outbox.IsStored)
                        {
                            var account = await _accountsRepository.GetAsync(requesterContext.AggregateId);
                            var accountDetails = AccountDetails.Create(
                                outbox.AggregateId,
                                new AccountDetailsId(evt.BlockchainId, evt.Address),
                                account.Id,
                                account.BrokerAccountId);

                            await account.AddAccountDetails(
                                _accountDetailsRepository, 
                                _accountsRepository,
                                accountDetails, 
                                requesterContext.ExpectedCount);

                            foreach (var item in account.Events)
                            {
                                outbox.Publish(item);
                            }

                            await _outboxManager.Store(outbox);
                        }

                        await _outboxManager.EnsureDispatched(outbox);

                        _logger.LogInformation("AccountDetails have been added {@context}", evt);

                        break;
                    }
                case AggregateType.BrokerAccount:
                    {
                        var outbox = await _outboxManager.Open($"BrokerAccountDetails:Create:{evt.WalletGenerationRequestId}",
                            () => _brokerAccountDetailsRepository.GetNextIdAsync());
                        if (!outbox.IsStored)
                        {
                            var brokerAccount = await _brokerAccountsRepository.GetAsync(requesterContext.AggregateId);
                            var brokerAccountDetails = BrokerAccountDetails.Create(
                                outbox.AggregateId,
                                new BrokerAccountDetailsId(evt.BlockchainId, evt.Address),
                                brokerAccount.TenantId,
                                brokerAccount.Id);

                            await brokerAccount.AddBrokerAccountDetails(
                                _brokerAccountDetailsRepository,
                                _brokerAccountsRepository,
                                brokerAccountDetails,
                                requesterContext.ExpectedCount);

                            foreach (var item in brokerAccount.Events)
                            {
                                outbox.Publish(item);
                            }

                            await _outboxManager.Store(outbox);
                        }

                        await _outboxManager.EnsureDispatched(outbox);

                        _logger.LogInformation("BrokerAccountDetails have been added {@context}", evt);

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(requesterContext.AggregateType), 
                        requesterContext.AggregateType, 
                        "This should not happen at all!");
            }
        }
    }
}
