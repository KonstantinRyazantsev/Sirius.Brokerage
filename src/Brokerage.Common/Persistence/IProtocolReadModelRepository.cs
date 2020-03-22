using System.Threading.Tasks;
using Brokerage.Common.Domain.Protocols;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Persistence
{
    public interface IProtocolReadModelRepository
    {
        Task<Protocol> GetOrDefaultAsync(ProtocolId prtocolId);

        Task<Protocol> AddOrReplaceAsync(Protocol protocol);
    }
}
