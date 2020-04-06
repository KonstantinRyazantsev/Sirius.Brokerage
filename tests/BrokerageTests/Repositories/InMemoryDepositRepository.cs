using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Deposits;
using Brokerage.Common.Persistence.Deposits;

namespace BrokerageTests.Repositories
{
    public class InMemoryDepositRepository : IDepositsRepository
    {
        private long _idCounter = 0;
        private readonly List<Deposit> _storage;

        public InMemoryDepositRepository()
        {
            _storage = new List<Deposit>();
        }

        public Task<Deposit> GetOrDefaultAsync(
            string transactionId,
            long assetId,
            long brokerAccountRequisitesId,
            long? accountRequisitesId)
        {
            return Task.FromResult(_storage.FirstOrDefault(x => x.TransactionInfo.TransactionId == transactionId &&
                                                x.AssetId == assetId &&
                                                x.BrokerAccountRequisitesId == brokerAccountRequisitesId &&
                                                x.AccountRequisitesId == accountRequisitesId));
        }

        public Task<long> GetNextIdAsync()
        {
            _idCounter++;

            return Task.FromResult(_idCounter);
        }

        public Task SaveAsync(Deposit deposit)
        {
            _storage.Add(Deposit.Restore(
                _idCounter,
                deposit.Version,
                deposit.Sequence,
                deposit.BrokerAccountRequisitesId,
                deposit.AccountRequisitesId,
                deposit.AssetId,
                deposit.Amount,
                deposit.Fees,
                deposit.TransactionInfo,
                deposit.Error,
                deposit.DepositState,
                deposit.Sources,
                deposit.Address,
                deposit.DetectedDateTime,
                deposit.ConfirmedDateTime,
                deposit.CompletedDateTime,
                deposit.FailedDateTime,
                deposit.CancelledDateTime));

            return Task.CompletedTask;
        }
    }
}
