using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
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

            var depositDetector = new  DepositsDetector(
                accountRequisitesRepository, 
                brokerAccountRequisitesRepository,
                brokerAccountsBalancesRepository,
                publishEndpoint);

            var bitcoinRegtest = "bitcoin-regtest";
            var brokerAccountRequisistes = BrokerAccountRequisites.Create("request-1", 100_000, bitcoinRegtest);
            brokerAccountRequisistes.Address = "address";
            var operationAmount = 15m;
            await brokerAccountRequisitesRepository.AddOrGetAsync(brokerAccountRequisistes);
            var detectedTransaction = new TransactionDetected()
            {
                BlockchainId = bitcoinRegtest,
                BalanceUpdates = new BalanceUpdate[]
                {
                    new BalanceUpdate()
                    {
                        Address = brokerAccountRequisistes.Address,
                        AssetId = 100_000,
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
        }
    }
}
