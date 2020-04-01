using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Indexer.MessagingContract;
using Xunit;

namespace BrokerageTests.UnitTests
{
    public class DepositDetectorTests
    {
        public DepositDetectorTests()
        {

        }

        [Fact]
        public async Task Test()
        {
            var accountRequisitesRepository = new InMemoryAccountRequisitesRepository();
            var brokerAccountRequisitesRepository = new InMemoryBrokerAccountRequisitesRepository();
            var brokerAccountsBalancesRepository = new InMemoryBrokerAccountsBalancesRepository();
            var publishEndpoint = new InMemoryPublishEndpoint();

            var depositDetector = new DepositsDetector(
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
            var detectedTransaction = new TransactionDetected()
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
                },
                BlockId = "BlockId#1",
                BlockNumber = 1,
                ErrorCode = null,
                ErrorMessage = null,
                Fees = new Fee[0],
                TransactionId = "TransactionId#1",
                TransactionNumber = 0,
            };

            await depositDetector.Detect(detectedTransaction);

            var brokerAccountBalancesUpdated = publishEndpoint.Events.First() as BrokerAccountBalancesUpdated;
            var balance = await brokerAccountsBalancesRepository.GetOrDefaultAsync(brokerAccountId, assetId);

            Assert.NotNull(brokerAccountBalancesUpdated);
            Assert.Equal(balance.PendingBalance, brokerAccountBalancesUpdated.PendingBalance);
            Assert.Equal(balance.AssetId, brokerAccountBalancesUpdated.AssetId);
            Assert.Equal(balance.Sequence, brokerAccountBalancesUpdated.Sequence);
            Assert.Equal(balance.PendingBalanceUpdateDateTime, brokerAccountBalancesUpdated.PendingBalanceUpdateDateTime);
        }
    }
}
