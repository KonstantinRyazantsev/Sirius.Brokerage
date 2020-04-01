﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.ServiceFunctions;
using MassTransit;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Worker.MessageConsumers
{
    public class PublishAccountRequisitesConsumer : IConsumer<PublishAccountRequisites>
    {
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;

        public PublishAccountRequisitesConsumer(IAccountRequisitesRepository accountRequisitesRepository)
        {
            _accountRequisitesRepository = accountRequisitesRepository;
        }

        public async Task Consume(ConsumeContext<PublishAccountRequisites> context)
        {
            var cursor = default(long?);

            do
            {
                var page = await _accountRequisitesRepository.GetAllAsync(cursor, 1000);

                if (!page.Any())
                {
                    break;
                }

                foreach (var requisites in page)
                {
                    var evt = new AccountRequisitesAdded
                    {
                        CreationDateTime = requisites.CreationDateTime,
                        Address = requisites.Address,
                        BlockchainId = requisites.BlockchainId,
                        Tag = requisites.Tag,
                        TagType = requisites.TagType.HasValue
                            ? requisites.TagType.Value switch
                            {
                                DestinationTagType.Number => TagType.Number,
                                DestinationTagType.Text => TagType.Text,
                                _ => throw new ArgumentOutOfRangeException(nameof(requisites.TagType),
                                    requisites.TagType,
                                    null)
                            }
                            : (TagType?) null,
                        AccountId = requisites.AccountId,
                        AccountRequisitesId = requisites.AccountRequisitesId
                    };

                    await context.Publish(evt);
                }

                cursor = page.Last().AccountRequisitesId;
            } while (true);
        }
    }
}
