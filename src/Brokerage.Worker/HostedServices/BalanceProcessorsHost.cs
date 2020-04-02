using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Common.Bilv1.DomainServices;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Worker.BalanceProcessors;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brokerage.Worker.HostedServices
{
    public class BalanceProcessorsHost : IHostedService, IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly BlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly IOperationRepository _operationRepository;
        private readonly IPublishEndpoint _eventPublisher;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly DepositsDetector _depositsDetector;
        private readonly DepositsConfirmator _depositsConfirmator;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly List<BalanceProcessorJob> _balanceReaders;

        public BalanceProcessorsHost(
            ILoggerFactory loggerFactory,
            BlockchainApiClientProvider blockchainApiClientProvider,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IOperationRepository operationRepository,
            IPublishEndpoint eventPublisher,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository,
            DepositsDetector depositsDetector,
            DepositsConfirmator depositsConfirmator,
            IBlockchainsRepository blockchainsRepository)
        {
            _loggerFactory = loggerFactory;
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _operationRepository = operationRepository;
            _eventPublisher = eventPublisher;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _accountRequisitesRepository = accountRequisitesRepository;
            _depositsDetector = depositsDetector;
            _depositsConfirmator = depositsConfirmator;
            _blockchainsRepository = blockchainsRepository;

            _balanceReaders = new List<BalanceProcessorJob>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var blockchain in await _blockchainsRepository.GetAllAsync())
            {
                var blockchainApiClient = await _blockchainApiClientProvider.Get(blockchain.BlockchainId);
                var blockchainAssetsDict = await blockchainApiClient.GetAllAssetsAsync(100);

                var balanceProcessor = new BalanceProcessor(
                    blockchain.BlockchainId,
                    _loggerFactory.CreateLogger<BalanceProcessor>(),
                    blockchainApiClient,
                    _enrolledBalanceRepository,
                    _operationRepository,
                    _eventPublisher,
                    _brokerAccountRequisitesRepository,
                    _accountRequisitesRepository,
                    _depositsDetector,
                    _depositsConfirmator,
                    blockchainAssetsDict);

                var balanceReader = new BalanceProcessorJob(
                    blockchain.BlockchainId,
                    _loggerFactory.CreateLogger<BalanceProcessorsHost>(),
                    balanceProcessor,
                    TimeSpan.FromSeconds(10));

                balanceReader.Start();

                _balanceReaders.Add(balanceReader);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var balanceReader in _balanceReaders)
            {
                balanceReader.Stop();
            }

            foreach (var balanceReader in _balanceReaders)
            {
                balanceReader.Wait();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var balanceReader in _balanceReaders)
            {
                balanceReader.Dispose();
            }
        }
    }
}
