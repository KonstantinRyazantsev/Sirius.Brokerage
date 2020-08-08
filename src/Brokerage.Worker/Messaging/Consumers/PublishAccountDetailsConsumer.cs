using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.ServiceFunctions;
using MassTransit;
using Swisschain.Sirius.Brokerage.MessagingContract.Accounts;

namespace Brokerage.Worker.Messaging.Consumers
{
    public class PublishAccountDetailsConsumer : IConsumer<PublishAccountDetails>
    {
        private readonly IAccountDetailsRepository _accountDetailsRepository;

        public PublishAccountDetailsConsumer(IAccountDetailsRepository accountDetailsRepository)
        {
            _accountDetailsRepository = accountDetailsRepository;
        }

        public async Task Consume(ConsumeContext<PublishAccountDetails> context)
        {
            var cursor = default(long?);

            do
            {
                var page = await _accountDetailsRepository.GetAllAsync(cursor, 1000);

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
