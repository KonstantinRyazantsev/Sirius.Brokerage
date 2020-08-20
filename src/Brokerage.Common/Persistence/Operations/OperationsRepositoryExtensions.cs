using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;

namespace Brokerage.Common.Persistence.Operations
{
    public static class OperationsRepositoryExtensions
    {
        public static async Task<Operation> GetOrDefault(this IOperationsRepository repo, long? id)
        {
            return id != null ? await repo.GetOrDefault(id.Value) : null;
        }
    }
}
