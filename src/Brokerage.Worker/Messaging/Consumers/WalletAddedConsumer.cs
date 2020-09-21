using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Limiters;
using Brokerage.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Indexer.ApiClient;
using Swisschain.Sirius.Indexer.ApiContract.ObservedAddresses;
using Swisschain.Sirius.VaultAgent.MessagingContract.Wallets;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class WalletAddedConsumer : IConsumer<WalletAdded>
    {
        private readonly ILogger<WalletAddedConsumer> _logger;
        private readonly IIndexerClient _indexerClient;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IIdGenerator _idGenerator;
        private readonly ConcurrencyLimiter _concurrencyLimiter;

        public WalletAddedConsumer(ILogger<WalletAddedConsumer> logger,
            IIndexerClient indexerClient,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IIdGenerator idGenerator,
            ConcurrencyLimiter concurrencyLimiter)
        {
            _logger = logger;
            _indexerClient = indexerClient;
            _unitOfWorkManager = unitOfWorkManager;
            _idGenerator = idGenerator;
            _concurrencyLimiter = concurrencyLimiter;
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

            await _indexerClient.ObservedAddresses.AddObservedAddressAsync(new AddObservedAddressRequest
            {
                RequestId = $"Brokerage:{evt.WalletId}",
                BlockchainId = evt.BlockchainId,
                Address = evt.Address
            });

            var requesterContext = Newtonsoft.Json.JsonConvert.DeserializeObject<WalletGenerationRequesterContext>(evt.Context);

            switch (requesterContext.WalletGenerationReason)
            {
                case WalletGenerationReason.Account:
                    await CreateAccountDetails(evt, requesterContext, context);
                    break;

                case WalletGenerationReason.BrokerAccount:
                    await CreateBrokerAccountDetails(evt, requesterContext, context);
                    break;

                case WalletGenerationReason.AccountUpdate:
                    await CreateAccountDetailsWithinUpdate(evt, requesterContext, context);
                    break;

                case WalletGenerationReason.BrokerAccountUpdate:
                    await CreateBrokerAccountDetails(evt, requesterContext, context);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(requesterContext.WalletGenerationReason), requesterContext.WalletGenerationReason, "");
            }
        }

        private async Task CreateAccountDetailsWithinUpdate(WalletAdded evt,
            WalletGenerationRequesterContext requesterContext,
            ConsumeContext<WalletAdded> consumeContext)
        {
            using var limit = await _concurrencyLimiter.Enter($"BrokerAccount:{requesterContext.RootId}");
            await using var unitOfWork = await _unitOfWorkManager.Begin($"AccountDetails:Create:{evt.WalletGenerationRequestId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var account = await unitOfWork.Accounts.Get(requesterContext.AggregateId);
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(account.BrokerAccountId);

                var accountDetailsId = await _idGenerator.GetId(unitOfWork.Outbox.IdempotencyId, IdGenerators.AccountDetails);
                var accountDetails = AccountDetails.Create(
                    accountDetailsId,
                    new AccountDetailsId(evt.BlockchainId, evt.Address),
                    account.Id,
                    account.BrokerAccountId);

                await account.AddAccountDetails(
                    unitOfWork.BrokerAccounts,
                    unitOfWork.AccountDetails,
                    unitOfWork.Accounts,
                    accountDetails,
                    brokerAccount,
                    requesterContext.ExpectedBlockchainsCount,
                    requesterContext.ExpectedAccountsCount);

                foreach (var item in account.Commands)
                {
                    unitOfWork.Outbox.Send(item);
                }

                foreach (var item in account.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                foreach (var item in brokerAccount.Commands)
                {
                    unitOfWork.Outbox.Send(item);
                }

                foreach (var item in brokerAccount.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(consumeContext);
        }

        private async Task CreateAccountDetails(WalletAdded evt,
            WalletGenerationRequesterContext requesterContext,
            ConsumeContext<WalletAdded> consumeContext)
        {
            using var limit = await _concurrencyLimiter.Enter($"BrokerAccount:{requesterContext.RootId}");
            await using var unitOfWork = await _unitOfWorkManager.Begin($"AccountDetails:Create:{evt.WalletGenerationRequestId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var account = await unitOfWork.Accounts.Get(requesterContext.AggregateId);
                var accountDetailsId = await _idGenerator.GetId(unitOfWork.Outbox.IdempotencyId, IdGenerators.AccountDetails);
                var accountDetails = AccountDetails.Create(
                    accountDetailsId,
                    new AccountDetailsId(evt.BlockchainId, evt.Address),
                    account.Id,
                    account.BrokerAccountId);

                await account.AddAccountDetails(
                    unitOfWork.AccountDetails,
                    unitOfWork.Accounts,
                    accountDetails,
                    requesterContext.ExpectedBlockchainsCount);

                foreach (var item in account.Commands)
                {
                    unitOfWork.Outbox.Send(item);
                }

                foreach (var item in account.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(consumeContext);
        }

        private async Task CreateBrokerAccountDetails(WalletAdded evt,
            WalletGenerationRequesterContext requesterContext,
            ConsumeContext<WalletAdded> consumeContext)
        {
            using var limit = await _concurrencyLimiter.Enter($"BrokerAccount:{requesterContext.AggregateId}");
            await using var unitOfWork = await _unitOfWorkManager.Begin($"BrokerAccountDetails:Create:{evt.WalletGenerationRequestId}");

            if (!unitOfWork.Outbox.IsClosed)
            {
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(requesterContext.AggregateId);
                var brokerAccountDetailsId = await _idGenerator.GetId(unitOfWork.Outbox.IdempotencyId, IdGenerators.BrokerAccountDetails);
                var brokerAccountDetails = BrokerAccountDetails.Create(
                    brokerAccountDetailsId,
                    new BrokerAccountDetailsId(evt.BlockchainId, evt.Address),
                    brokerAccount.TenantId,
                    brokerAccount.Id);

                await brokerAccount.AddBrokerAccountDetails(
                    unitOfWork.BrokerAccountDetails,
                    unitOfWork.BrokerAccounts,
                    brokerAccountDetails,
                    requesterContext.ExpectedBlockchainsCount,
                    requesterContext.ExpectedAccountsCount);

                foreach (var item in brokerAccount.Commands)
                {
                    unitOfWork.Outbox.Send(item);
                }

                foreach (var item in brokerAccount.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(consumeContext);
        }
    }
}
