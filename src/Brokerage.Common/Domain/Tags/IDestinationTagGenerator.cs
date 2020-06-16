using System.Threading.Tasks;

namespace Brokerage.Common.Domain.Tags
{
    public interface IDestinationTagGenerator
    {
        string Generate();
    }
}
