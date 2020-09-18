using System;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Limiters;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Blockchains;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class CreateAccountDetailsForTagConsumer : IConsumer<CreateAccountDetailsForTag>
    {
        private readonly ILogger<CreateAccountDetailsForTagConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IIdGenerator _idGenerator;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;
        private readonly ConcurrencyLimiter _concurrencyLimiter;

        public CreateAccountDetailsForTagConsumer(
            ILogger<CreateAccountDetailsForTagConsumer> logger,
            IBlockchainsRepository blockchainsRepository,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IIdGenerator idGenerator,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory,
            ConcurrencyLimiter concurrencyLimiter)
        {
            _logger = logger;
            _blockchainsRepository = blockchainsRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _idGenerator = idGenerator;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
            _concurrencyLimiter = concurrencyLimiter;
        }

        public async Task Consume(ConsumeContext<CreateAccountDetailsForTag> context)
        {
            var command = context.Message;

            using var limit = await _concurrencyLimiter.Enter($"BrokerAccount:{command.BrokerAccountId}");
            await using var unitOfWork = await _unitOfWorkManager.Begin($"AccountDetails:Create:{command.AccountId}:{command.BlockchainId}");
            
            if (!unitOfWork.Outbox.IsClosed)
            {
                var blockchain = await _blockchainsRepository.GetAsync(command.BlockchainId);
                var destinationTagType = DestinationTagType.Number;
                var tagGenerator = _destinationTagGeneratorFactory.Create(blockchain);
                var tag = tagGenerator.Generate();
                var account = await unitOfWork.Accounts.Get(command.AccountId);
                var brokerAccount = await unitOfWork.BrokerAccounts.Get(account.BrokerAccountId);
                var brokerAccountDetails = await unitOfWork.BrokerAccountDetails.GetActive(
                    new ActiveBrokerAccountDetailsId(blockchain.Id, account.BrokerAccountId));

                if (brokerAccountDetails == null)
                {
                    _logger.LogWarning("No broker account details {@context}", command);
                    await context.Redeliver(TimeSpan.FromSeconds(30));

                    return;
                }

                var id = await _idGenerator.GetId($"AccountDetails:{account.Id}:{blockchain.Id}", IdGenerators.AccountDetails);
                
                var accountDetails = AccountDetails.Create(
                    id,
                    new AccountDetailsId(blockchain.Id,
                        brokerAccountDetails.NaturalId.Address,
                        tag,
                        destinationTagType),
                    account.Id,
                    account.BrokerAccountId);

                await account.AddAccountDetails(
                    unitOfWork.BrokerAccounts,
                    unitOfWork.AccountDetails,
                    unitOfWork.Accounts,
                    accountDetails,
                    brokerAccount,
                    command.ExpectedBlockchainsCount,
                    command.ExpectedAccountsCount);

                foreach (var item in account.Commands)
                {
                    unitOfWork.Outbox.Send(item);
                }

                foreach (var item in account.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                foreach(var item in brokerAccount.Commands)
                {
                    unitOfWork.Outbox.Send(item);
                }

                foreach (var item in brokerAccount.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
