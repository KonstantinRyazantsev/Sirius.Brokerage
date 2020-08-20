using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Persistence;
using Brokerage.Common.ServiceFunctions;
using MassTransit;
using Swisschain.Extensions.Idempotency;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class PublishAccountDetailsConsumer : IConsumer<PublishAccountDetails>
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;

        public PublishAccountDetailsConsumer(IUnitOfWorkManager<UnitOfWork> unitOfWorkManager)
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task Consume(ConsumeContext<PublishAccountDetails> context)
        {
            await using var unitOfWork = await _unitOfWorkManager.Begin();

            var cursor = default(long?);

            do
            {
                var page = await unitOfWork.AccountDetails.GetAll(cursor, 1000);

                if (!page.Any())
                {
                    break;
                }

                foreach (var accountDetails in page)
                {
                    var evt = new AccountDetailsAdded
                    {
                        CreatedAt = accountDetails.CreatedAt,
                        Address = accountDetails.NaturalId.Address,
                        BlockchainId = accountDetails.NaturalId.BlockchainId,
                        Tag = accountDetails.NaturalId.Tag,
                        TagType = accountDetails.NaturalId.TagType,
                        AccountId = accountDetails.AccountId,
                        AccountDetailsId = accountDetails.Id
                    };

                    await context.Publish(evt);
                }

                cursor = page.Last().Id;
            } while (true);
        }
    }
}
