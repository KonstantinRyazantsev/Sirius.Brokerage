using Brokerage.Common.ReadModels.Blockchains;

namespace Brokerage.Common.Domain.Tags
{
    public interface IDestinationTagGeneratorFactory
    {
        IDestinationTagGenerator CreateOrDefault(Blockchain blockchain);
        IDestinationTagGenerator Create(Blockchain blockchain);
    }
}
