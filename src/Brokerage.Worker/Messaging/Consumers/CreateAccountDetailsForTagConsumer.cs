using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Tags;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Blockchains;
using MassTransit;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class CreateAccountDetailsForTagConsumer : IConsumer<CreateAccountDetailsForTag>
    {
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IIdGenerator _idGenerator;
        private readonly IDestinationTagGeneratorFactory _destinationTagGeneratorFactory;

        public CreateAccountDetailsForTagConsumer(
            IBlockchainsRepository blockchainsRepository,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IIdGenerator idGenerator,
            IDestinationTagGeneratorFactory destinationTagGeneratorFactory)
        {
            _blockchainsRepository = blockchainsRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _idGenerator = idGenerator;
            _destinationTagGeneratorFactory = destinationTagGeneratorFactory;
        }

        public async Task Consume(ConsumeContext<CreateAccountDetailsForTag> context)
        {
            var command = context.Message;
            
            await using var unitOfWork = await _unitOfWorkManager.Begin($"AccountDetails:Create:{command.AccountId}:{command.BlockchainId}");
            
            if (!unitOfWork.Outbox.IsClosed)
            {
                var blockchain = await _blockchainsRepository.GetAsync(command.BlockchainId);
                var destinationTagType = DestinationTagType.Number;
                var tagGenerator = _destinationTagGeneratorFactory.Create(blockchain);
                var tag = tagGenerator.Generate();
                var account = await unitOfWork.Accounts.Get(command.AccountId);
                var brokerAccountDetails = await unitOfWork.BrokerAccountDetails.GetActive(
                    new ActiveBrokerAccountDetailsId(blockchain.Id, account.BrokerAccountId));
                
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
                    unitOfWork.AccountDetails,
                    unitOfWork.Accounts,
                    accountDetails,
                    command.ExpectedCount);

                foreach (var item in account.Events)
                {
                    unitOfWork.Outbox.Publish(item);
                }

                await unitOfWork.Commit();
            }

            await unitOfWork.EnsureOutboxDispatched(context);
        }
    }
}
