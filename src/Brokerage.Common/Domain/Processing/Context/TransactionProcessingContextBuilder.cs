using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.Threading;
using Swisschain.Extensions.Idempotency;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class TransactionProcessingContextBuilder
    {
        private readonly IAccountRequisitesRepository _accountRequisitesRepository;
        private readonly IBrokerAccountRequisitesRepository _brokerAccountRequisitesRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IOutboxManager _outboxManager;

        public TransactionProcessingContextBuilder(IAccountRequisitesRepository accountRequisitesRepository,
            IBrokerAccountRequisitesRepository brokerAccountRequisitesRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IDepositsRepository depositsRepository,
            IOperationsRepository operationsRepository,
            IOutboxManager outboxManager)
        {
            _accountRequisitesRepository = accountRequisitesRepository;
            _brokerAccountRequisitesRepository = brokerAccountRequisitesRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _depositsRepository = depositsRepository;
            _operationsRepository = operationsRepository;
            _outboxManager = outboxManager;
        }

        public async Task<TransactionProcessingContext> Build(string blockchainId,
            long? operationId,
            TransactionInfo transactionInfo,
            SourceContext[] sources,
            DestinationContext[] destinations)
        {
            var sourceAddresses = sources
                .Select(x => x.Address)
                .Distinct()
                .ToHashSet();
            var sourceAccountRequisitesIds = sourceAddresses
                .Select(x => new AccountRequisitesId(blockchainId, x))
                .ToHashSet();
            var sourceBrokerAccountRequisitesIds = sourceAddresses
                .Select(x => new BrokerAccountRequisitesId(blockchainId, x))
                .ToHashSet();

            var destinationAccountRequisitesIds = destinations
                .Select(x => new AccountRequisitesId(blockchainId, x.Address, x.Tag, x.TagType))
                .Distinct()
                .ToHashSet();
            var destinationAddresses = destinationAccountRequisitesIds
                .Select(x => x.Address)
                .Distinct()
                .ToHashSet();
            var destinationBrokerAccountRequisitesIds = destinationAddresses
                .Select(x => new BrokerAccountRequisitesId(blockchainId, x))
                .ToHashSet();
            
            var allAccountRequisitesIds = sourceAccountRequisitesIds
                .Union(destinationAccountRequisitesIds)
                .ToHashSet();
            var allBrokerAccountRequisitesIds = sourceBrokerAccountRequisitesIds
                .Union(destinationBrokerAccountRequisitesIds)
                .ToHashSet();

            var (matchedAccountRequisites, matchedBrokerAccountRequisites, matchedOperation) = await TaskExecution.WhenAll(
                _accountRequisitesRepository.GetAnyOfAsync(allAccountRequisitesIds),
                _brokerAccountRequisitesRepository.GetAnyOfAsync(allBrokerAccountRequisitesIds),
                _operationsRepository.GetOrDefaultAsync(operationId));

            var matchedBrokerAccountIds = matchedBrokerAccountRequisites
                .Select(x => x.BrokerAccountId)
                .Union(matchedAccountRequisites.Select(x => x.BrokerAccountId))
                .ToArray();

            var matchedAddress = matchedAccountRequisites
                .Select(x => x.NaturalId.Address)
                .Union(matchedBrokerAccountRequisites.Select(x => x.NaturalId.Address))
                .ToHashSet();

            var matchedSources = sources
                .Where(x => matchedAddress.Contains(x.Address))
                .ToArray();
            var matchedDestinations = destinations
                .Where(x => matchedAddress.Contains(x.Address))
                .ToArray();

            var brokerAccountBalancesIds = BuildBrokerAccountBalancesIds(
                matchedBrokerAccountIds, 
                matchedAccountRequisites, 
                matchedBrokerAccountRequisites,
                matchedSources,
                matchedDestinations);

            var activeBrokerAccountRequisitesIds = matchedBrokerAccountIds
                .Select(x => new ActiveBrokerAccountRequisitesId(blockchainId, x))
                .ToHashSet();

            var (existingBrokerAccountBalances, activeBrokerAccountRequisites, deposits) = await TaskExecution.WhenAll(
                _brokerAccountsBalancesRepository.GetAnyOfAsync(brokerAccountBalancesIds),
                _brokerAccountRequisitesRepository.GetActiveAsync(activeBrokerAccountRequisitesIds),
                _depositsRepository.GetAllByTransactionAsync(blockchainId, transactionInfo.TransactionId));
            var newBrokerAccountBalances = await BuildNewBrokerAccountBalances(existingBrokerAccountBalances, brokerAccountBalancesIds);
            var brokerAccountBalances = existingBrokerAccountBalances
                .Concat(newBrokerAccountBalances)
                .ToArray();

            var brokerAccountsContext = matchedBrokerAccountIds
                .Select(x => BuildBrokerAccountContext(x,
                    blockchainId,
                    activeBrokerAccountRequisites,
                    brokerAccountBalances,
                    matchedBrokerAccountRequisites,
                    matchedDestinations,
                    matchedSources,
                    matchedAccountRequisites))
                .ToArray();

            return new TransactionProcessingContext(
                brokerAccountsContext,
                matchedOperation,
                transactionInfo,
                deposits);
        }

        private BrokerAccountContext BuildBrokerAccountContext(long brokerAccountId,
            string blockchainId,
            IReadOnlyDictionary<ActiveBrokerAccountRequisitesId, BrokerAccountRequisites> allActiveRequisites,
            IReadOnlyCollection<BrokerAccountBalances> brokerAccountBalances,
            IReadOnlyCollection<BrokerAccountRequisites> matchedBrokerAccountRequisites,
            IReadOnlyCollection<DestinationContext> matchedDestinations,
            IReadOnlyCollection<SourceContext> matchedSources,
            IReadOnlyCollection<AccountRequisites> matchedAccountRequisites)
        {
            var brokerAccountRequisites = matchedBrokerAccountRequisites
                .Where(x => x.BrokerAccountId == brokerAccountId)
                .ToArray();

            var inputs = new List<BrokerAccountContextEndpoint>();
            var outputs = new List<BrokerAccountContextEndpoint>();

            foreach (var requisites in brokerAccountRequisites)
            {
                var requisitesInputs = matchedDestinations
                    .Where(x => x.Address == requisites.NaturalId.Address)
                    .Select(x => new BrokerAccountContextEndpoint(requisites, x.Unit))
                    .ToArray();
                var requisitesOutputs = matchedSources
                    .Where(x => x.Address == requisites.NaturalId.Address)
                    .Select(x => new BrokerAccountContextEndpoint(requisites, x.Unit))
                    .ToArray();

                inputs.AddRange(requisitesInputs);
                outputs.AddRange(requisitesOutputs);
            }

            var income = inputs
                .GroupBy(x => x.Unit.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Unit.Amount));
            var outcome = outputs
                .GroupBy(x => x.Unit.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Unit.Amount));

            var accounts = matchedAccountRequisites
                .Select(x => BuildAccountContext(
                    matchedDestinations, 
                    matchedSources, 
                    x))
                .ToArray();

            var balances = brokerAccountBalances
                .Select(x => new BrokerAccountBalancesContext(
                    x,
                    income.GetValueOrDefault(x.NaturalId.AssetId),
                    outcome.GetValueOrDefault(x.NaturalId.AssetId)))
                .ToArray();

            var activeRequisites = allActiveRequisites[new ActiveBrokerAccountRequisitesId(blockchainId, brokerAccountId)];

            return new BrokerAccountContext(
                activeRequisites.TenantId,
                activeRequisites.BrokerAccountId,
                activeRequisites,
                accounts,
                balances,
                inputs,
                outputs,
                income,
                outcome);
        }

        private static AccountContext BuildAccountContext(
            IReadOnlyCollection<DestinationContext> matchedDestinations,
            IReadOnlyCollection<SourceContext> matchedSources,
            AccountRequisites requisites)
        {
            var inputs = matchedDestinations
                .Where(x => x.Address == requisites.NaturalId.Address)
                .Select(x => x.Unit)
                .ToArray();
            var outputs = matchedSources
                .Where(x => x.Address == requisites.NaturalId.Address)
                .Select(x => x.Unit)
                .ToArray();
            var income = inputs
                .GroupBy(x => x.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
            var outcome = outputs
                .GroupBy(x => x.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            return new AccountContext(requisites, inputs, outputs, income, outcome);
        }

        private async Task<IReadOnlyCollection<BrokerAccountBalances>> BuildNewBrokerAccountBalances(
            IEnumerable<BrokerAccountBalances> existingBrokerAccountBalances,
            IEnumerable<BrokerAccountBalancesId> brokerAccountBalancesIds)
        {
            var existingBrokerAccountBalancesIds = existingBrokerAccountBalances
                .Select(x => x.NaturalId)
                .ToHashSet();
            var notExistingBrokerAccountBalancesIds = brokerAccountBalancesIds.Where(x => !existingBrokerAccountBalancesIds.Contains(x));
            var newBrokerAccountBalances = new List<BrokerAccountBalances>();

            foreach (var brokerAccountBalancesId in notExistingBrokerAccountBalancesIds)
            {
                // TODO: Decouple outbox and ID generator
                var outbox = await _outboxManager.Open(
                    $"BrokerAccountBalances:Create:{brokerAccountBalancesId.BrokerAccountId}_{brokerAccountBalancesId.AssetId}",
                    () => _brokerAccountsBalancesRepository.GetNextIdAsync());

                newBrokerAccountBalances.Add(BrokerAccountBalances.Create(outbox.AggregateId, brokerAccountBalancesId));
            }

            return newBrokerAccountBalances;
        }

        private static ISet<BrokerAccountBalancesId> BuildBrokerAccountBalancesIds(
            IReadOnlyCollection<long> matchedBrokerAccountIds,
            IReadOnlyCollection<AccountRequisites> matchedAccountRequisites,
            IReadOnlyCollection<BrokerAccountRequisites> matchedBrokerAccountRequisites,
            IReadOnlyCollection<SourceContext> matchedSources,
            IReadOnlyCollection<DestinationContext> matchedDestinations)
        {
            var brokerAccountBalancesIds = new List<BrokerAccountBalancesId>();

            foreach (var brokerAccountId in matchedBrokerAccountIds)
            {
                var accountAddressesOfTheBrokerAccount = matchedAccountRequisites
                    .Where(x => x.BrokerAccountId == brokerAccountId)
                    .Select(x => x.NaturalId.Address);
                var brokerAccountAddressesOfTheBrokerAccount = matchedBrokerAccountRequisites
                    .Where(x => x.BrokerAccountId == brokerAccountId)
                    .Select(x => x.NaturalId.Address);
                var allAddressesOfTheBrokerAccount = accountAddressesOfTheBrokerAccount
                    .Union(brokerAccountAddressesOfTheBrokerAccount)
                    .ToHashSet();

                var matchedAssetIds = matchedSources
                    .Where(x => allAddressesOfTheBrokerAccount.Contains(x.Address))
                    .Select(x => x.Unit.AssetId)
                    .Union(matchedDestinations
                        .Where(x => allAddressesOfTheBrokerAccount.Contains(x.Address))
                        .Select(x => x.Unit.AssetId))
                    .ToHashSet();

                brokerAccountBalancesIds.AddRange(matchedAssetIds.Select(assetId =>
                    new BrokerAccountBalancesId(brokerAccountId, assetId)));
            }

            return brokerAccountBalancesIds.ToHashSet();
        }
    }
}
