//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Brokerage.Common.Domain.Accounts;
//using Brokerage.Common.Domain.BrokerAccounts;
//using Brokerage.Common.Domain.Deposits;
//using BrokerageTests.Repositories;
//using Shouldly;
//using Swisschain.Sirius.Brokerage.MessagingContract;
//using Swisschain.Sirius.Indexer.MessagingContract;
//using Swisschain.Sirius.Sdk.Primitives;
//using Xunit;
//using DepositState = Swisschain.Sirius.Brokerage.MessagingContract.DepositState;

//namespace BrokerageTests.UnitTests
//{
//    public class DepositDetectorTests
//    {
//        [Fact(Skip = "Broker account deposits are not supported")]
//        public async Task SingleTransferTest()
//        {
//            var AccountDetailsRepository = new InMemoryAccountDetailsRepository();
//            var brokerAccountDetailsRepository = new InMemoryBrokerAccountDetailsRepository();
//            var brokerAccountsBalancesRepository = new InMemoryBrokerAccountsBalancesRepository();
//            var publishEndpoint = new InMemoryPublishEndpoint();
//            var depositRepository = new InMemoryDepositRepository();

//            var depositDetector = new DepositsDetector(
//                AccountDetailsRepository,
//                brokerAccountDetailsRepository,
//                brokerAccountsBalancesRepository,
//                publishEndpoint,
//                depositRepository);

//            var bitcoinRegtest = "bitcoin-regtest";
//            var brokerAccountId = 100_000;
//            var senderAddress = "sender-address";
//            var brokerAccountRequisistes = BrokerAccountDetails.Create("request-1", brokerAccountId, bitcoinRegtest);
//            brokerAccountRequisistes.Address = "receiver-address";
//            var operationAmount = 15m;
//            brokerAccountRequisistes = await brokerAccountDetailsRepository.AddOrGetAsync(brokerAccountRequisistes);
//            var assetId = 100_000;
//            var detectedTransaction = new TransactionDetected
//            {
//                BlockchainId = bitcoinRegtest,
//                Destinations = new []
//                {
//                    new TransferDestination
//                    {
//                        Address = brokerAccountRequisistes.Address,
//                        Unit = new Unit(assetId, operationAmount)
//                    }
//                },
//                Sources = new []
//                {
//                    new TransferSource
//                    {
//                        Address = senderAddress,
//                        Unit = new Unit(assetId, operationAmount)
//                    }
//                },
//                BlockId = "BlockId#1",
//                BlockNumber = 1,
//                Error = null,
//                Fees = Array.Empty<Unit>(),
//                TransactionId = "TransactionId#1",
//                TransactionNumber = 0,
//            };

//            await depositDetector.Detect(detectedTransaction);

//            var brokerAccountBalancesUpdated = publishEndpoint.Events.First() as BrokerAccountBalancesUpdated;
//            var balance = await brokerAccountsBalancesRepository.GetOrDefaultAsync(brokerAccountId, assetId);
//            var deposit = await depositRepository.GetOrDefaultAsync(detectedTransaction.TransactionId,
//                assetId,
//                brokerAccountRequisistes.Id,
//                null);

//            brokerAccountBalancesUpdated.ShouldNotBeNull();

//            balance.PendingBalance.ShouldBe(brokerAccountBalancesUpdated.PendingBalance);
//            balance.AssetId.ShouldBe(brokerAccountBalancesUpdated.AssetId);
//            balance.Sequence.ShouldBe(brokerAccountBalancesUpdated.Sequence);
//            balance.PendingBalanceUpdatedAt.ShouldBe(brokerAccountBalancesUpdated.PendingBalanceUpdatedAt);

//            deposit.ShouldNotBeNull();
//            var depositUpdated = publishEndpoint.Events.First(x => x is DepositUpdated) as DepositUpdated;

//            depositUpdated.ShouldNotBeNull();
//            depositUpdated.AssetId.ShouldBe(assetId);
//            depositUpdated.AccountDetailsId.ShouldBeNull();
//            depositUpdated.Amount.ShouldBe(operationAmount);
//            depositUpdated.BrokerAccountDetailsId.ShouldBe(brokerAccountRequisistes.Id);
//            depositUpdated.DepositId.ShouldBe(deposit.Id);
//            depositUpdated.State.ShouldBe(DepositState.Detected);
//            depositUpdated.Sources.ShouldNotBeNull();

//            // TODO: Note this is not present for Bil V1 as fees
//            var source = depositUpdated.Sources.First();

//            source.Address.ShouldBe(senderAddress);
//            source.Amount.ShouldBe(operationAmount);
//        }

//        [Fact]
//        public async Task MultipleTransfersTest()
//        {
//            var AccountDetailsRepository = new InMemoryAccountDetailsRepository();
//            var brokerAccountDetailsRepository = new InMemoryBrokerAccountDetailsRepository();
//            var brokerAccountsBalancesRepository = new InMemoryBrokerAccountsBalancesRepository();
//            var publishEndpoint = new InMemoryPublishEndpoint();
//            var depositRepository = new InMemoryDepositRepository();

//            var depositDetector = new DepositsDetector(
//                AccountDetailsRepository,
//                brokerAccountDetailsRepository,
//                brokerAccountsBalancesRepository,
//                publishEndpoint,
//                depositRepository);

//            var bitcoinRegtest = "bitcoin-regtest";
//            var brokerAccountId = 100_000;
//            var accountId = 100_000;
//            var brokerAccountRequisistes = BrokerAccountDetails.Create("request-1", brokerAccountId, bitcoinRegtest);
//            var address2 = "address2";
//            var accountRequisistes = AccountDetails.Create(
//                "request-1",
//                accountId,
//                brokerAccountId,
//                bitcoinRegtest,
//                address2);
//            brokerAccountRequisistes.Address = "address";
//            var operationAmount = 15m;
//            brokerAccountRequisistes = await brokerAccountDetailsRepository.AddOrGetAsync(brokerAccountRequisistes);
//            accountRequisistes = await AccountDetailsRepository.AddOrGetAsync(accountRequisistes);

//            var assetId = 100_000;
//            var assetId2 = 200_000;
//            var detectedTransaction = new TransactionDetected()
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
//                        Address = brokerAccountRequisistes.Address,
//                        Unit = new Unit(assetId2, 2 * operationAmount)
//                    },
//                    new TransferDestination
//                    {
//                        Address = accountRequisistes.Address,
//                        Unit = new Unit(assetId, operationAmount)
//                    },
//                    new TransferDestination
//                    {
//                        Address = accountRequisistes.Address,
//                        Unit = new Unit(assetId2, 2 * operationAmount)
//                    }
//                },
//                Sources = Array.Empty<TransferSource>(),
//                BlockId = "BlockId#1",
//                BlockNumber = 1,
//                Error = null,
//                Fees = Array.Empty<Unit>(),
//                TransactionId = "TransactionId#1",
//                TransactionNumber = 0,
//            };

//            await depositDetector.Detect(detectedTransaction);

//            var brokerAccountBalancesUpdates = publishEndpoint
//                .Events
//                ?.Select(x => x as BrokerAccountBalancesUpdated)
//                .Where(x => x != null)
//                .ToArray();

//            brokerAccountBalancesUpdates.ShouldNotBeNull();

//            foreach (var item in brokerAccountBalancesUpdates)
//            {
//                var balance = await brokerAccountsBalancesRepository.GetOrDefaultAsync(item.BrokerAccountId, item.AssetId);

//                brokerAccountBalancesUpdates.ShouldNotBeNull();
//                balance.PendingBalance.ShouldBe(item.PendingBalance);
//                balance.AssetId.ShouldBe(item.AssetId);
//                balance.Sequence.ShouldBe(item.Sequence);
//                balance.PendingBalanceUpdatedAt.ShouldBe(item.PendingBalanceUpdatedAt);
//            }

//            var depositUpdates = publishEndpoint
//                .Events
//                ?.Select(x => x as DepositUpdated)
//                .Where(x => x != null)
//                .ToArray();

//            foreach (var depositUpdate in depositUpdates)
//            {
//                var depositUpdateFromStorage = await depositRepository.GetOrDefaultAsync(
//                    depositUpdate.TransactionInfo.TransactionId,
//                    depositUpdate.AssetId,
//                    depositUpdate.BrokerAccountDetailsId,
//                    depositUpdate.AccountDetailsId);

//                depositUpdateFromStorage.ShouldNotBeNull();
//                depositUpdate.AssetId.ShouldBe(depositUpdateFromStorage.AssetId);
//                depositUpdate.Sequence.ShouldBe(depositUpdateFromStorage.Sequence);
//                depositUpdate.AccountDetailsId.ShouldBe(depositUpdateFromStorage.AccountDetailsId);
//                depositUpdate.BrokerAccountDetailsId.ShouldBe(depositUpdateFromStorage.BrokerAccountDetailsId);
//                depositUpdate.State.ShouldBe(DepositState.Detected);
//                depositUpdate.Amount.ShouldBe(depositUpdateFromStorage.Amount);
//                depositUpdate.DetectedAt.ShouldBe(depositUpdateFromStorage.DetectedAt);
//                depositUpdate.TransactionInfo.TransactionId.ShouldBe(depositUpdateFromStorage.TransactionInfo.TransactionId);
//                depositUpdate.TransactionInfo.TransactionBlock.ShouldBe(depositUpdateFromStorage.TransactionInfo.TransactionBlock);
//                depositUpdate.TransactionInfo.RequiredConfirmationsCount.ShouldBe(depositUpdateFromStorage.TransactionInfo.RequiredConfirmationsCount);
//                depositUpdate.BrokerAccountDetailsId.ShouldBe(depositUpdateFromStorage.BrokerAccountDetailsId);
//            }
//        }
//    }
//}
