using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Assets;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccount;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Brokerage.Common.ReadModels.Assets;
using Brokerage.Common.Threading;
using Swisschain.Extensions.Idempotency;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class TransactionProcessingContextBuilder
    {
        private readonly IAccountDetailsRepository _accountDetailsRepository;
        private readonly IBrokerAccountDetailsRepository _brokerAccountDetailsRepository;
        private readonly IBrokerAccountsBalancesRepository _brokerAccountsBalancesRepository;
        private readonly IDepositsRepository _depositsRepository;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IOutboxManager _outboxManager;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IAssetsRepository _assetsRepository;

        public TransactionProcessingContextBuilder(IAccountDetailsRepository accountDetailsRepository,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IDepositsRepository depositsRepository,
            IOperationsRepository operationsRepository,
            IOutboxManager outboxManager,
            IBlockchainsRepository blockchainsRepository,
            IAssetsRepository assetsRepository)
        {
            _accountDetailsRepository = accountDetailsRepository;
            _brokerAccountDetailsRepository = brokerAccountDetailsRepository;
            _brokerAccountsBalancesRepository = brokerAccountsBalancesRepository;
            _depositsRepository = depositsRepository;
            _operationsRepository = operationsRepository;
            _outboxManager = outboxManager;
            _blockchainsRepository = blockchainsRepository;
            _assetsRepository = assetsRepository;
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
            var sourceAccountDetailsIds = sourceAddresses
                .Select(x => new AccountDetailsId(blockchainId, x))
                .ToHashSet();
            var sourceBrokerAccountDetailsIds = sourceAddresses
                .Select(x => new BrokerAccountDetailsId(blockchainId, x))
                .ToHashSet();

            var destinationAccountDetailsIds = destinations
                .Select(x => new AccountDetailsId(blockchainId, x.Address, x.Tag, x.TagType))
                .Distinct()
                .ToHashSet();
            var destinationAddresses = destinationAccountDetailsIds
                .Select(x => x.Address)
                .Distinct()
                .ToHashSet();
            var destinationBrokerAccountDetailsIds = destinationAddresses
                .Select(x => new BrokerAccountDetailsId(blockchainId, x))
                .ToHashSet();
            
            var allAccountDetailsIds = sourceAccountDetailsIds
                .Union(destinationAccountDetailsIds)
                .ToHashSet();
            var allBrokerAccountDetailsIds = sourceBrokerAccountDetailsIds
                .Union(destinationBrokerAccountDetailsIds)
                .ToHashSet();

            var (matchedAccountDetails, matchedBrokerAccountDetails, matchedOperation) = await TaskExecution.WhenAll(
                _accountDetailsRepository.GetAnyOfAsync(allAccountDetailsIds),
                _brokerAccountDetailsRepository.GetAnyOfAsync(allBrokerAccountDetailsIds),
                _operationsRepository.GetOrDefaultAsync(operationId));

            var matchedBrokerAccountIds = matchedBrokerAccountDetails
                .Select(x => x.BrokerAccountId)
                .Union(matchedAccountDetails.Select(x => x.BrokerAccountId))
                .ToArray();

            var matchedAddress = matchedAccountDetails
                .Select(x => x.NaturalId.Address)
                .Union(matchedBrokerAccountDetails.Select(x => x.NaturalId.Address))
                .ToHashSet();

            var matchedSources = sources
                .Where(x => matchedAddress.Contains(x.Address))
                .ToArray();
            var matchedDestinations = destinations
                .Where(x => matchedAddress.Contains(x.Address))
                .ToArray();

            var brokerAccountBalancesIds = BuildBrokerAccountBalancesIds(
                matchedBrokerAccountIds, 
                matchedAccountDetails, 
                matchedBrokerAccountDetails,
                matchedSources,
                matchedDestinations);

            var activeBrokerAccountDetailsIds = matchedBrokerAccountIds
                .Select(x => new ActiveBrokerAccountDetailsId(blockchainId, x))
                .ToHashSet();

            var (existingBrokerAccountBalances, activeBrokerAccountDetails, deposits, blockchain) = await TaskExecution.WhenAll(
                _brokerAccountsBalancesRepository.GetAnyOfAsync(brokerAccountBalancesIds),
                _brokerAccountDetailsRepository.GetActiveAsync(activeBrokerAccountDetailsIds),
                _depositsRepository.Search(
                    blockchainId,
                    transactionInfo.TransactionId,
                    consolidationOperationId: matchedOperation?.Type == OperationType.DepositConsolidation ? (long?)matchedOperation.Id : null),
                _blockchainsRepository.GetAsync(blockchainId));
            var newBrokerAccountBalances = await BuildNewBrokerAccountBalances(existingBrokerAccountBalances, brokerAccountBalancesIds);
            var brokerAccountBalances = existingBrokerAccountBalances
                .Concat(newBrokerAccountBalances)
                .ToArray();

            //var allAssetsForInputs = matchedDestinations.Select(x => x.Unit.AssetId)
            //    .Distinct()
            //    .ToArray();
            //var assets = (await _assetsRepository.GetByManyIds(allAssetsForInputs))
            //    .ToDictionary(x => x.Id);

            var brokerAccountsContext = matchedBrokerAccountIds
                .Select(x => BuildBrokerAccountContext(x,
                    blockchainId,
                    activeBrokerAccountDetails,
                    brokerAccountBalances,
                    matchedBrokerAccountDetails,
                    matchedDestinations,
                    matchedSources,
                    matchedAccountDetails))
                .ToArray();

            return new TransactionProcessingContext(
                brokerAccountsContext,
                matchedOperation,
                transactionInfo,
                deposits,
                blockchain);
        }

        private BrokerAccountContext BuildBrokerAccountContext(long brokerAccountId,
            string blockchainId,
            IReadOnlyDictionary<ActiveBrokerAccountDetailsId, BrokerAccountDetails> allActiveDetails,
            IReadOnlyCollection<BrokerAccountBalances> brokerAccountBalances,
            IReadOnlyCollection<BrokerAccountDetails> matchedBrokerAccountDetails,
            IReadOnlyCollection<DestinationContext> matchedDestinations,
            IReadOnlyCollection<SourceContext> matchedSources,
            IReadOnlyCollection<AccountDetails> matchedAccountDetails)
        {
            var brokerAccountDetails = matchedBrokerAccountDetails
                .Where(x => x.BrokerAccountId == brokerAccountId)
                .ToArray();

            var inputs = new List<BrokerAccountContextEndpoint>();
            var outputs = new List<BrokerAccountContextEndpoint>();

            foreach (var accountDetails in brokerAccountDetails)
            {
                var detailsInput = matchedDestinations
                    .Where(x => x.Address == accountDetails.NaturalId.Address)
                    .Select(x => new BrokerAccountContextEndpoint(accountDetails, x.Unit))
                    .ToArray();
                var detailsOutput = matchedSources
                    .Where(x => x.Address == accountDetails.NaturalId.Address)
                    .Select(x => new BrokerAccountContextEndpoint(accountDetails, x.Unit))
                    .ToArray();

                inputs.AddRange(detailsInput);
                outputs.AddRange(detailsOutput);
            }

            var brokerAccountByBlockchainDict =
                brokerAccountDetails.ToDictionary(x => (x.BrokerAccountId, x.NaturalId.BlockchainId));

            var income = inputs
                .GroupBy(x => new Tuple<long, long>(x.Details.Id, x.Unit.AssetId))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Unit.Amount));
            var outcome = outputs
                .GroupBy(x => x.Unit.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Unit.Amount));

            var accounts = matchedAccountDetails
                .Select(x => BuildAccountContext(
                    matchedDestinations, 
                    matchedSources, 
                    x))
                .ToArray();

            var balances = brokerAccountBalances
                .Select(x =>
                {
                    brokerAccountByBlockchainDict
                        .TryGetValue((x.NaturalId.BrokerAccountId, blockchainId), out var brokerAccountDetailsForBalance);
                    var incomeKey = new Tuple<long, long>(brokerAccountDetailsForBalance.Id, x.NaturalId.AssetId);

                    return new BrokerAccountBalancesContext(
                        x,
                        income.GetValueOrDefault(incomeKey),
                            outcome.GetValueOrDefault(x.NaturalId.AssetId),
                            x.NaturalId.AssetId);
                }).ToArray();

            var activeDetails = allActiveDetails[new ActiveBrokerAccountDetailsId(blockchainId, brokerAccountId)];

            return new BrokerAccountContext(
                activeDetails.TenantId,
                activeDetails.BrokerAccountId,
                activeDetails,
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
            AccountDetails details)
        {
            var inputs = matchedDestinations
                .Where(x => x.Address == details.NaturalId.Address)
                .Select(x => x.Unit)
                .ToArray();
            var outputs = matchedSources
                .Where(x => x.Address == details.NaturalId.Address)
                .Select(x => x.Unit)
                .ToArray();
            var income = inputs
                .GroupBy(x => x.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
            var outcome = outputs
                .GroupBy(x => x.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            return new AccountContext(details, inputs, outputs, income, outcome);
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
            IReadOnlyCollection<AccountDetails> matchedAccountDetails,
            IReadOnlyCollection<BrokerAccountDetails> matchedBrokerAccountDetails,
            IReadOnlyCollection<SourceContext> matchedSources,
            IReadOnlyCollection<DestinationContext> matchedDestinations)
        {
            var brokerAccountBalancesIds = new List<BrokerAccountBalancesId>();

            foreach (var brokerAccountId in matchedBrokerAccountIds)
            {
                var accountAddressesOfTheBrokerAccount = matchedAccountDetails
                    .Where(x => x.BrokerAccountId == brokerAccountId)
                    .Select(x => x.NaturalId.Address);
                var brokerAccountAddressesOfTheBrokerAccount = matchedBrokerAccountDetails
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
