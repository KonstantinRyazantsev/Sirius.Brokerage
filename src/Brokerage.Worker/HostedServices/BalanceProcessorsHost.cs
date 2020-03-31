using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Repositories;
using Brokerage.Bilv1.DomainServices;
using Brokerage.Common.Configuration;
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
        private readonly IntegrationsConfig _integrationsConfig;
        private readonly BlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly IEnrolledBalanceRepository _enrolledBalanceRepository;
        private readonly IOperationRepository _operationRepository;
        private readonly IPublishEndpoint _eventPublisher;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly DepositsDetector _depositsDetector;
        private readonly WithdrawalsDetector _withdrawalsDetector;
        private readonly List<BalanceProcessorJob> _balanceReaders;

        public BalanceProcessorsHost(
            ILoggerFactory loggerFactory,
            IntegrationsConfig integrationsConfig,
            BlockchainApiClientProvider blockchainApiClientProvider,
            IEnrolledBalanceRepository enrolledBalanceRepository,
            IOperationRepository operationRepository,
            IPublishEndpoint eventPublisher,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IAccountRequisitesRepository accountRequisitesRepository,
            DepositsDetector depositsDetector,
            WithdrawalsDetector withdrawalsDetector)
        {
            _loggerFactory = loggerFactory;
            _integrationsConfig = integrationsConfig;
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _enrolledBalanceRepository = enrolledBalanceRepository;
            _operationRepository = operationRepository;
            _eventPublisher = eventPublisher;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _accountRequisitesRepository = accountRequisitesRepository;
            _depositsDetector = depositsDetector;
            _withdrawalsDetector = withdrawalsDetector;

            _balanceReaders = new List<BalanceProcessorJob>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var blockchain in _integrationsConfig.Blockchains)
            {
                var blockchainApiClient = _blockchainApiClientProvider.Get(blockchain.Id);
                var blockchainAssetsDict = await blockchainApiClient.GetAllAssetsAsync(100);

                var balanceProcessor = new BalanceProcessor(
                    blockchain.Id,
                    blockchain.NetworkId,
                    _loggerFactory.CreateLogger<BalanceProcessor>(),
                    blockchainApiClient,
                    _enrolledBalanceRepository,
                    _operationRepository,
                    _eventPublisher,
                    _brokerAccountRequisitesRepository,
                    _accountRequisitesRepository,
                    _depositsDetector,
                    _withdrawalsDetector,
                    blockchainAssetsDict);

                var balanceReader = new BalanceProcessorJob(
                    blockchain.Id,
                    _loggerFactory.CreateLogger<BalanceProcessorsHost>(),
                    balanceProcessor,
                    _integrationsConfig.BalanceUpdatePeriod);

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
