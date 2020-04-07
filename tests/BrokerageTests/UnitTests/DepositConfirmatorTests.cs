using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brokerage.Common.Domain;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using BrokerageTests.Repositories;
using Grpc.Core;
using Microsoft.VisualBasic;
using Shouldly;
using Swisschain.Sirius.Brokerage.MessagingContract;
using Swisschain.Sirius.Confirmator.MessagingContract;
using Swisschain.Sirius.Executor.ApiContract.Monitoring;
using Swisschain.Sirius.Executor.ApiContract.Transfers;
using Xunit;
using BalanceUpdate = Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate;
using DepositSource = Brokerage.Common.Domain.Deposits.DepositSource;
using DepositState = Swisschain.Sirius.Brokerage.MessagingContract.DepositState;
using Fee = Swisschain.Sirius.Confirmator.MessagingContract.Fee;
using TransactionInfo = Brokerage.Common.Domain.TransactionInfo;
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
            var depositRepository = new InMemoryDepositRepository();
            var publishEndpoint = new InMemoryPublishEndpoint();
            Swisschain.Sirius.Executor.ApiClient.IExecutorClient executorClient = new 
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository = new InMemoryBrokerAccountRequisitesRepository();
            IAccountRequisitesRepository accountRequisitesRepository = new InMemoryAccountRequisitesRepository();


            var depositsConfirmator = new DepositsConfirmator(depositRepository, publishEndpoint);
            var bitcoinRegtest = "bitcoin-regtest";
            var brokerAccountId = 100_000;
            var address = "address";
            var operationAmount = 15m;
            var assetId = 100_000;
            var detectedTransaction = new TransactionConfirmed()
            {
                BlockchainId = bitcoinRegtest,
                BalanceUpdates = new Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate[]
                {
                    new Swisschain.Sirius.Confirmator.MessagingContract.BalanceUpdate()
                    {
                        Address = address,
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

            var depositCreate = Deposit.Create(
                await depositRepository.GetNextIdAsync(),
                brokerAccountId,
                1,
                assetId,
                1m,
                new TransactionInfo("TransactionId#1", 1, 1, DateTime.UtcNow),
                Array.Empty<DepositSource>());
            await depositRepository.SaveAsync(depositCreate);
            await depositsConfirmator.Confirm(detectedTransaction);

            var depositUpdated = publishEndpoint.Events.Last() as DepositUpdated;

            depositUpdated.ShouldNotBeNull();
            depositUpdated.ConfirmedDateTime.ShouldNotBeNull();
            depositUpdated.State.ShouldBe(DepositState.Confirmed);
        }
    }

    public class ExecutorClient : Swisschain.Sirius.Executor.ApiClient.IExecutorClient
    {
        public Monitoring.MonitoringClient Monitoring { get; }
        public Transfers.TransfersClient Transfers { get; }

        public class TestTransfersClient : Transfers.TransfersClient
        {
            public override ExecuteTransferResponse Execute(ExecuteTransferRequest request,
                Metadata headers = null,
                DateTime? deadline = null,
                CancellationToken cancellationToken = default)
            {

            }

            public override ExecuteTransferResponse Execute(ExecuteTransferRequest request, CallOptions options)
            {

            }

            public override AsyncUnaryCall<ExecuteTransferResponse> ExecuteAsync(ExecuteTransferRequest request,
                Metadata headers = null,
                DateTime? deadline = null,
                CancellationToken cancellationToken = default)
            {
                return new AsyncUnaryCall<ExecuteTransferResponse>();
            }

            public override AsyncUnaryCall<ExecuteTransferResponse> ExecuteAsync(ExecuteTransferRequest request,
                CallOptions options)
            {

            }
        }
    }
}

