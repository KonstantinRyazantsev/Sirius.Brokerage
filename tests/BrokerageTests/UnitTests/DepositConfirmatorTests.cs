using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using BrokerageTests.Repositories;
using Shouldly;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Xunit;
using BalanceUpdate = Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate;
using Fee = Swisschain.Sirius.Confirmator.MessagingContract.Fee;
using Transfer = Swisschain.Sirius.Confirmator.MessagingContract.Transfer;

namespace BrokerageTests.UnitTests
{
    public class DepositConfirmatorTests
    {
        public DepositConfirmatorTests()
        {

        }

        [Fact]
        public async Task SingleTransferTest()
        {
            var accountRequisitesRepository = new InMemoryAccountRequisitesRepository();
            var brokerAccountRequisitesRepository = new InMemoryBrokerAccountRequisitesRepository();
            var brokerAccountsBalancesRepository = new InMemoryBrokerAccountsBalancesRepository();
            var publishEndpoint = new InMemoryPublishEndpoint();

            var depositDetector = new DepositsConfirmator(
                accountRequisitesRepository,
                brokerAccountRequisitesRepository,
                brokerAccountsBalancesRepository,
                publishEndpoint);

            var bitcoinRegtest = "bitcoin-regtest";
            var brokerAccountId = 100_000;
            var brokerAccountRequisistes = BrokerAccountRequisites.Create("request-1", brokerAccountId, bitcoinRegtest);
            brokerAccountRequisistes.Address = "address";
            var operationAmount = 15m;
            await brokerAccountRequisitesRepository.AddOrGetAsync(brokerAccountRequisistes);
            var assetId = 100_000;
            var detectedTransaction = new TransactionConfirmed()
            {
                BlockchainId = bitcoinRegtest,
                BalanceUpdates = new Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate[]
                {
                    new Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate()
                    {
                        Address = brokerAccountRequisistes.Address,
                        AssetId = assetId,
                        Transfers = new List<Transfer>()
                        {
                            new Transfer()
                            {
                                Amount = operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                },
                BlockId = "BlockId#1",
                BlockNumber = 1,
                ErrorCode = null,
                ErrorMessage = null,
                Fees = new Fee[0],
                TransactionId = "TransactionId#1",
                TransactionNumber = 0,
            };

            await depositDetector.Confirm(detectedTransaction);

            var brokerAccountBalancesUpdated = publishEndpoint.Events.First() as BrokerAccountBalancesUpdated;
            var balance = await brokerAccountsBalancesRepository.GetOrDefaultAsync(brokerAccountId, assetId);

            brokerAccountBalancesUpdated.ShouldNotBeNull();
            brokerAccountBalancesUpdated.OwnedBalance.ShouldBeGreaterThan(0m);
            brokerAccountBalancesUpdated.AvailableBalance.ShouldBeGreaterThan(0m);
            balance.OwnedBalance.ShouldBe(brokerAccountBalancesUpdated.OwnedBalance);
            balance.PendingBalance.ShouldBeLessThan(balance.OwnedBalance);
            balance.AvailableBalance.ShouldBe(brokerAccountBalancesUpdated.AvailableBalance);
            balance.AssetId.ShouldBe(brokerAccountBalancesUpdated.AssetId);
            balance.Sequence.ShouldBe(brokerAccountBalancesUpdated.Sequence);
            balance.PendingBalanceUpdateDateTime.ShouldBe(brokerAccountBalancesUpdated.PendingBalanceUpdateDateTime);
            balance.OwnedBalanceUpdateDateTime.ShouldBe(brokerAccountBalancesUpdated.OwnedBalanceUpdateDateTime);
            balance.AvailableBalanceUpdateDateTime.ShouldBe(brokerAccountBalancesUpdated.AvailableBalanceUpdateDateTime);
        }

        [Fact]
        public async Task MultipleTransfersTest()
        {
            var accountRequisitesRepository = new InMemoryAccountRequisitesRepository();
            var brokerAccountRequisitesRepository = new InMemoryBrokerAccountRequisitesRepository();
            var brokerAccountsBalancesRepository = new InMemoryBrokerAccountsBalancesRepository();
            var publishEndpoint = new InMemoryPublishEndpoint();

            var depositDetector = new DepositsConfirmator(
                accountRequisitesRepository,
                brokerAccountRequisitesRepository,
                brokerAccountsBalancesRepository,
                publishEndpoint);

            var bitcoinRegtest = "bitcoin-regtest";
            var brokerAccountId = 100_000;
            var accountId = 100_000;
            var address2 = "address2";
            var accountRequisistes = AccountRequisites.Create(
                "request-1", 
                accountId, 
                brokerAccountId, 
                bitcoinRegtest, 
                address2);
            var brokerAccountRequisistes = BrokerAccountRequisites.Create("request-1", brokerAccountId, bitcoinRegtest);
            brokerAccountRequisistes.Address = "address";
            var operationAmount = 15m;
            brokerAccountRequisistes = await brokerAccountRequisitesRepository.AddOrGetAsync(brokerAccountRequisistes);
            accountRequisistes = await accountRequisitesRepository.AddOrGetAsync(accountRequisistes);

            var assetId = 100_000;
            var assetId2 = 200_000;
            var detectedTransaction = new TransactionConfirmed()
            {
                BlockchainId = bitcoinRegtest,
                BalanceUpdates = new BalanceUpdate[]
                {
                    new BalanceUpdate()
                    {
                        Address = brokerAccountRequisistes.Address,
                        AssetId = assetId,
                        Transfers = new List<Transfer>()
                        {
                            new Transfer()
                            {
                                Amount = operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                    new BalanceUpdate()
                    {
                        Address = brokerAccountRequisistes.Address,
                        AssetId = assetId2,
                        Transfers = new List<Transfer>()
                        {
                            new Transfer()
                            {
                                Amount = 2 * operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                    new BalanceUpdate()
                    {
                        Address = accountRequisistes.Address,
                        AssetId = assetId,
                        Transfers = new List<Transfer>()
                        {
                            new Transfer()
                            {
                                Amount = operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                    new BalanceUpdate()
                    {
                        Address = accountRequisistes.Address,
                        AssetId = assetId2,
                        Transfers = new List<Transfer>()
                        {
                            new Transfer()
                            {
                                Amount = 2 * operationAmount,
                                TransferId = 0,
                                Nonce = 0
                            }
                        }
                    },
                },
                BlockId = "BlockId#1",
                BlockNumber = 1,
                ErrorCode = null,
                ErrorMessage = null,
                Fees = new Fee[0],
                TransactionId = "TransactionId#1",
                TransactionNumber = 0,
            };

            await depositDetector.Confirm(detectedTransaction);

            var brokerAccountBalancesUpdates = publishEndpoint
                .Events
                ?.Select(x => x as BrokerAccountBalancesUpdated)
                .ToArray();

            brokerAccountBalancesUpdates.ShouldNotBeNull();
            brokerAccountBalancesUpdates.Length.ShouldBe(2);

            {
                var item = brokerAccountBalancesUpdates[0];
                var balance = await brokerAccountsBalancesRepository.GetOrDefaultAsync(item.BrokerAccountId, item.AssetId);

                brokerAccountBalancesUpdates.ShouldNotBeNull();
                item.OwnedBalance.ShouldBeGreaterThan(0m);
                item.AvailableBalance.ShouldBeGreaterThan(0m);
                balance.OwnedBalance.ShouldBe(item.OwnedBalance);
                balance.AvailableBalance.ShouldBe(item.AvailableBalance);
                balance.AssetId.ShouldBe(item.AssetId);
                balance.Sequence.ShouldBe(item.Sequence);
                balance.PendingBalanceUpdateDateTime.ShouldBe(item.PendingBalanceUpdateDateTime);
                balance.OwnedBalanceUpdateDateTime.ShouldBe(item.OwnedBalanceUpdateDateTime);
                balance.AvailableBalanceUpdateDateTime.ShouldBe(item.AvailableBalanceUpdateDateTime);
            }

            {
                var item = brokerAccountBalancesUpdates[1];
                var balance = await brokerAccountsBalancesRepository.GetOrDefaultAsync(item.BrokerAccountId, item.AssetId);

                brokerAccountBalancesUpdates.ShouldNotBeNull();
                item.OwnedBalance.ShouldBeGreaterThan(0m);
                balance.OwnedBalance.ShouldBe(item.OwnedBalance);
                balance.AssetId.ShouldBe(item.AssetId);
                balance.Sequence.ShouldBe(item.Sequence);
                balance.PendingBalanceUpdateDateTime.ShouldBe(item.PendingBalanceUpdateDateTime);
                balance.OwnedBalanceUpdateDateTime.ShouldBe(item.OwnedBalanceUpdateDateTime);
            }
        }
    }
}

