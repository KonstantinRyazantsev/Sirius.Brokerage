//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Brokerage.Common.Domain.Accounts;
//using Brokerage.Common.Domain.BrokerAccounts;
//using Brokerage.Common.Domain.Deposits;
//using Brokerage.Common.Persistence.Accounts;
//using Brokerage.Common.Persistence.BrokerAccount;
//using BrokerageTests.Repositories;
//using Shouldly;
//using Swisschain.Sirius.Brokerage.MessagingContract;
//using Swisschain.Sirius.Confirmator.MessagingContract;
//using Swisschain.Sirius.Executor.ApiContract.Common;
//using Xunit;
//using DepositSource = Brokerage.Common.Domain.Deposits.DepositSource;
//using DepositState = Swisschain.Sirius.Brokerage.MessagingContract.DepositState;
//using TransactionInfo = Brokerage.Common.Domain.TransactionInfo;
//using Unit = Swisschain.Sirius.Sdk.Primitives.Unit;

//namespace BrokerageTests.UnitTests
//{
//    public class DepositConfirmatorTests
//    {
//        public DepositConfirmatorTests()
//        {
//        }

//        [Fact]
//        public async Task SingleTransferTest()
//        {
//            var depositRepository = new InMemoryDepositRepository();
//            var publishEndpoint = new InMemoryPublishEndpoint();
//            Swisschain.Sirius.Executor.ApiClient.IExecutorClient executorClient = new FakeExecutorClient();
//            var transferClient = executorClient.Transfers as FakeExecutorClient.TestTransfersClient;
//            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository = new InMemoryBrokerAccountRequisitesRepository();
//            IAccountRequisitesRepository accountRequisitesRepository = new InMemoryAccountRequisitesRepository();
//            var brokerAccountRepository = new InMemoryBrokerAccountRepository();

//            var depositsConfirmator = new DepositsConfirmator(
//                depositRepository,
//                executorClient,
//                brokerAccountRequisitesRepository,
//                accountRequisitesRepository,
//                brokerAccountRepository,
//                publishEndpoint);

//            var tenantId = "tenant";
//            var brokerAccount = BrokerAccount.Create("name", tenantId, "request");
//            brokerAccount = await brokerAccountRepository.AddOrGetAsync(brokerAccount);
//            var bitcoinRegtest = "bitcoin-regtest";
//            var brokerAccountId = brokerAccount.BrokerAccountId;
//            var accountId = 100_000;
//            var brokerAccountRequisistes = BrokerAccountRequisites.Create("request-1", brokerAccountId, bitcoinRegtest);
//            var address2 = "address2";
//            var accountRequisistes = AccountRequisites.Create(
//                "request-1",
//                accountId,
//                brokerAccountId,
//                bitcoinRegtest,
//                address2);
//            brokerAccountRequisistes.Address = "address";
//            var operationAmount = 15m;
//            brokerAccountRequisistes = await brokerAccountRequisitesRepository.AddOrGetAsync(brokerAccountRequisistes);
//            accountRequisistes = await accountRequisitesRepository.AddOrGetAsync(accountRequisistes);

//            var assetId = 100_000;
//            var detectedTransaction = new TransactionConfirmed
//            {
//                BlockchainId = bitcoinRegtest,
//                Destinations = new []
//                {
//                    new TransferDestination
//                    {
//                        Address = brokerAccountRequisistes.Address,
//                        Unit = new Unit(assetId, operationAmount)
//                    },
//                    new TransferDestination
//                    {
//                        Address = accountRequisistes.Address,
//                        Unit = new Unit(assetId, operationAmount),
//                    }
//                },
//                BlockId = "BlockId#1",
//                BlockNumber = 1,
//                Error = null,
//                Fees = Array.Empty<Unit>(),
//                TransactionId = "TransactionId#1",
//                TransactionNumber = 0,
//                RequiredConfirmationsCount = 20
//            };

//            var depositCreate = Deposit.Create(
//                await depositRepository.GetNextIdAsync(),
//                brokerAccountRequisistes.Id,
//                accountRequisistes.Id,
//                assetId,
//                operationAmount,
//                new TransactionInfo("TransactionId#1", 1, 1, DateTime.UtcNow),
//                Array.Empty<DepositSource>());
//            await depositRepository.SaveAsync(depositCreate);
//            await depositsConfirmator.Confirm(detectedTransaction);

//            var depositsUpdated = 
//                publishEndpoint
//                    .Events
//                    .OfType<DepositUpdated>();

//            foreach (var depositUpdate in depositsUpdated)
//            {
//                depositUpdate.ShouldNotBeNull();
//                depositUpdate.ConfirmedAt.ShouldNotBeNull();
//                depositUpdate.State.ShouldBe(DepositState.Confirmed);
//            }

//            // ReSharper disable once PossibleNullReferenceException
//            var transfer = transferClient.TransferRequests.First();

//            transfer.Operation.TenantId.ShouldBe(tenantId);
//            var movement = transfer.Movements.First();
//            movement.SourceAddress.ShouldBe(accountRequisistes.Address);
//            movement.DestinationAddress.ShouldBe(brokerAccountRequisistes.Address);
//            BigDecimal transferAmount = operationAmount;
//            movement.Amount.ShouldBe(transferAmount);
//        }
//    }
//}

