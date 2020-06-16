using System.Threading.Tasks;
using Brokerage.Common.ReadModels.Blockchains;
using Swisschain.Sirius.Sdk.Primitives;

namespace Brokerage.Common.Domain.Tags
{
    public interface IDestinationTagGeneratorFactory
    {
        IDestinationTagGenerator CreateOrDefault(Blockchain blockchain);

        IDestinationTagGenerator Create(Blockchain blockchain);
    }
}
