using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.BrokerAccounts;
using Brokerage.Common.Domain.Operations;
using Brokerage.Common.Persistence;
using Brokerage.Common.Persistence.Accounts;
using Brokerage.Common.Persistence.Blockchains;
using Brokerage.Common.Persistence.BrokerAccounts;
using Brokerage.Common.Persistence.Deposits;
using Brokerage.Common.Persistence.Operations;
using Swisschain.Extensions.Idempotency;

namespace Brokerage.Common.Domain.Processing.Context
{
    public sealed class TransactionProcessingContextBuilder
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IBlockchainsRepository _blockchainsRepository;

        public TransactionProcessingContextBuilder(IIdGenerator idGenerator,
            IBlockchainsRepository blockchainsRepository)
        {
            _idGenerator = idGenerator;
            _blockchainsRepository = blockchainsRepository;
        }

        public async Task<TransactionProcessingContext> Build(string blockchainId,
            long? operationId,
            TransactionInfo transactionInfo,
            SourceContext[] sources,
            DestinationContext[] destinations,
            IBrokerAccountsRepository brokerAccountsRepository,
            IAccountDetailsRepository accountDetailsRepository,
            IBrokerAccountDetailsRepository brokerAccountDetailsRepository,
            IBrokerAccountsBalancesRepository brokerAccountsBalancesRepository,
            IDepositsRepository depositsRepository,
            IOperationsRepository operationsRepository,
            IMinDepositResidualsRepository minDepositResidualsRepository,
            IAccountsRepository accountsRepository)
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
            var matchedBrokerAccountDetailsIds = sourceBrokerAccountDetailsIds
                .Union(destinationBrokerAccountDetailsIds)
                .ToHashSet();

            var matchedAccountDetails = await accountDetailsRepository.GetAnyOf(allAccountDetailsIds);
            var matchedBrokerAccountDetails = await brokerAccountDetailsRepository.GetAnyOf(matchedBrokerAccountDetailsIds);
            var matchedOperation = await operationsRepository.GetOrDefault(operationId);
            var matchedAccounts = await accountsRepository.GetAnyOf(matchedAccountDetails.Select(x => x.AccountId).ToArray());

            if (matchedOperation == null &&
                !matchedAccountDetails.Any() &&
                !matchedBrokerAccountDetails.Any())
            {
                return TransactionProcessingContext.Empty;
            }

            var matchedBrokerAccountIds = matchedBrokerAccountDetails
                .Select(x => x.BrokerAccountId)
                .Union(matchedAccountDetails.Select(x => x.BrokerAccountId))
                .ToHashSet();

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

            var allBrokerAccountDetails = new HashSet<BrokerAccountDetails>(matchedBrokerAccountDetails, new BrokerAccountDetailsIdComparer());

            var activeBrokerAccountDetailsIds = matchedBrokerAccountIds
                .Select(x => new ActiveBrokerAccountDetailsId(blockchainId, x))
                .ToHashSet();

            var existingBrokerAccountBalances = await brokerAccountsBalancesRepository.GetAnyOf(brokerAccountBalancesIds);
            var activeBrokerAccountDetails = await brokerAccountDetailsRepository.GetActive(activeBrokerAccountDetailsIds);

            allBrokerAccountDetails.AddRange(activeBrokerAccountDetails.Values);

            var matchedBrokerAccounts = await brokerAccountsRepository.GetAllOf(matchedBrokerAccountIds);
            var deposits = await depositsRepository.Search(
                    blockchainId,
                    transactionInfo.TransactionId,
                    consolidationOperationId: matchedOperation?.Type == OperationType.DepositConsolidation ? (long?)matchedOperation.Id : null);
            var blockchain = await _blockchainsRepository.GetAsync(blockchainId);
            var newBrokerAccountBalances = await BuildNewBrokerAccountBalances(existingBrokerAccountBalances, brokerAccountBalancesIds);
            var brokerAccountBalances = existingBrokerAccountBalances
                .Concat(newBrokerAccountBalances)
                .ToArray();
            var depositBrokerAccountDetailIdsToLoad = deposits
                .Select(x => x.BrokerAccountDetailsId)
                .Except(allBrokerAccountDetails.Select(x => x.Id))
                .ToHashSet();
            var depositBrokerAccountDetails = await brokerAccountDetailsRepository.GetAnyOf(depositBrokerAccountDetailIdsToLoad);
            var minimumDepositResiduals = await minDepositResidualsRepository
                .GetAnyOfForUpdate(matchedAccountDetails.Select(x => x.NaturalId).ToHashSet());

            allBrokerAccountDetails.AddRange(depositBrokerAccountDetails);

            var brokerAccountsContext = matchedBrokerAccounts
                .Select(x => BuildBrokerAccountContext(x,
                    blockchainId,
                    activeBrokerAccountDetails,
                    matchedBrokerAccountDetails,
                    allBrokerAccountDetails,
                    brokerAccountBalances,
                    matchedDestinations,
                    matchedSources,
                    matchedAccountDetails))
                .ToArray();

            return new TransactionProcessingContext(
                brokerAccountsContext,
                matchedOperation,
                transactionInfo,
                deposits,
                blockchain,
                minimumDepositResiduals,
                matchedAccounts);
        }

        private static BrokerAccountContext BuildBrokerAccountContext(BrokerAccount brokerAccount,
            string blockchainId,
            IReadOnlyDictionary<ActiveBrokerAccountDetailsId, BrokerAccountDetails> activeBrokerAccountDetails,
            IReadOnlyCollection<BrokerAccountDetails> matchedBrokerAccountDetails,
            IReadOnlyCollection<BrokerAccountDetails> allBrokerAccountDetails,
            IReadOnlyCollection<BrokerAccountBalances> brokerAccountBalances,
            IReadOnlyCollection<DestinationContext> matchedDestinations,
            IReadOnlyCollection<SourceContext> matchedSources,
            IReadOnlyCollection<AccountDetails> matchedAccountDetails)
        {
            var currentMatchedBrokerAccountDetails = matchedBrokerAccountDetails
                .Where(x => x.BrokerAccountId == brokerAccount.Id)
                .ToArray();
            var currentAllBrokerAccountDetails = allBrokerAccountDetails
                .Where(x => x.BrokerAccountId == brokerAccount.Id)
                .ToDictionary(x => x.Id);

            var inputs = new List<BrokerAccountContextEndpoint>();
            var outputs = new List<BrokerAccountContextEndpoint>();

            foreach (var details in currentMatchedBrokerAccountDetails)
            {
                var detailsInput = matchedDestinations
                    .Where(x => x.Address == details.NaturalId.Address)
                    .Select(x => new BrokerAccountContextEndpoint(details, x.Unit))
                    .ToArray();
                var detailsOutput = matchedSources
                    .Where(x => x.Address == details.NaturalId.Address)
                    .Select(x => new BrokerAccountContextEndpoint(details, x.Unit))
                    .ToArray();

                inputs.AddRange(detailsInput);
                outputs.AddRange(detailsOutput);
            }

            var brokerAccountByBlockchainDict = currentMatchedBrokerAccountDetails.ToDictionary(x => (x.BrokerAccountId, x.NaturalId.BlockchainId));

            var income = inputs
                .GroupBy(x => (x.Details.Id, x.Unit.AssetId))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Unit.Amount));
            var outcome = outputs
                .GroupBy(x => x.Unit.AssetId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Unit.Amount));

            var accounts = matchedAccountDetails
                .Where(x => x.BrokerAccountId == brokerAccount.Id)
                .Select(x => BuildAccountContext(
                    matchedDestinations,
                    matchedSources,
                    x))
                .ToArray();

            var balances = brokerAccountBalances
                .Where(x => x.NaturalId.BrokerAccountId == brokerAccount.Id)
                .Select(x =>
                {
                    (long, long) incomeKey;

                    if (brokerAccountByBlockchainDict
                        .TryGetValue((x.NaturalId.BrokerAccountId, blockchainId), out var brokerAccountDetailsForBalance))
                    {
                        incomeKey = (brokerAccountDetailsForBalance.Id, x.NaturalId.AssetId);
                    }
                    else if (activeBrokerAccountDetails.TryGetValue(new ActiveBrokerAccountDetailsId(blockchainId, x.NaturalId.BrokerAccountId),
                            out var activeBrokerAccountDetailsForBalance))
                    {
                        incomeKey = (activeBrokerAccountDetailsForBalance.Id, x.NaturalId.AssetId);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Error happened. " +
                                                            $"It is not possible to find " +
                                                            $"broker account details for broker account {brokerAccount.Id}");
                    }

                    return new BrokerAccountBalancesContext(
                    x,
                    income.GetValueOrDefault(incomeKey),
                        outcome.GetValueOrDefault(x.NaturalId.AssetId),
                        x.NaturalId.AssetId);
                }).ToArray();

            var activeDetails = activeBrokerAccountDetails[new ActiveBrokerAccountDetailsId(blockchainId, brokerAccount.Id)];

            return new BrokerAccountContext(
                activeDetails.TenantId,
                activeDetails.BrokerAccountId,
                brokerAccount,
                activeDetails,
                accounts,
                balances,
                inputs,
                outputs,
                income,
                outcome,
                currentMatchedBrokerAccountDetails.ToDictionary(x => x.Id),
                currentAllBrokerAccountDetails);
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

            foreach (var brokerAccountBalancesNaturalId in notExistingBrokerAccountBalancesIds)
            {
                var brokerAccountBalancesId = await _idGenerator.GetId(
                    $"BrokerAccountBalances:{brokerAccountBalancesNaturalId.BrokerAccountId}_{brokerAccountBalancesNaturalId.AssetId}",
                    IdGenerators.BrokerAccountsBalances);

                newBrokerAccountBalances.Add(BrokerAccountBalances.Create(brokerAccountBalancesId, brokerAccountBalancesNaturalId));
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
