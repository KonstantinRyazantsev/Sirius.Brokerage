using System.Threading.Tasks;
using Brokerage.Common.Domain;

namespace Brokerage.Common.Persistence.Operations
{
    public static class OperationsRepositoryExtensions
    {
        public static async Task<Operation> GetOrDefaultAsync(this IOperationsRepository repo, long? id)
        {
            return id != null ? await repo.GetOrDefaultAsync(id.Value) : null;
        }
    }
}
