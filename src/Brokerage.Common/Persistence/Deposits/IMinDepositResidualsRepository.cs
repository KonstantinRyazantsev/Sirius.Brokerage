using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Accounts;
using Brokerage.Common.Domain.Deposits;

namespace Brokerage.Common.Persistence.Deposits
{
    public interface IMinDepositResidualsRepository
    {
        //Sort ids before select for update
        Task<IReadOnlyCollection<MinDepositResidual>> GetAnyOfForUpdate(ISet<AccountDetailsId> ids);
        Task Save(IReadOnlyCollection<MinDepositResidual> newMinDepositResiduals);
        Task Remove(IReadOnlyCollection<MinDepositResidual> prevMinDepositResiduals);
        Task<IReadOnlyCollection<MinDepositResidual>> GetForConsolidationDeposits(HashSet<long> consolidationDeposits);
    }
}
