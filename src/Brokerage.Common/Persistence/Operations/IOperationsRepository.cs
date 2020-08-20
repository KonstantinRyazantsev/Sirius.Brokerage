using System.Collections.Generic;
using System.Threading.Tasks;
using Brokerage.Common.Domain.Operations;

namespace Brokerage.Common.Persistence.Operations
{
    public interface IOperationsRepository
    {
        Task<Operation> GetOrDefault(long id);
        Task Add(Operation operation);
        Task Add(IReadOnlyCollection<Operation> operations);
        Task Update(Operation operation);
    }
}
