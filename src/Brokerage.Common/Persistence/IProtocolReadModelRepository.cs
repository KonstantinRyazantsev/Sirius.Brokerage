using System.Threading.Tasks;
using Brokerage.Common.Domain.Protocols;

namespace Brokerage.Common.Persistence
{
    public interface IProtocolReadModelRepository
    {
        Task<Protocol> AddOrReplaceAsync(Protocol protocol);
    }
}
