using System.Threading.Tasks;
using Brokerage.Common.Domain;

namespace Brokerage.Common.Persistence.Operations
{
    public interface IOperationsRepository
    {
        Task<Operation> GetOrDefaultAsync(long id);
    }
}
