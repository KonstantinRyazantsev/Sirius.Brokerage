using System.Threading.Tasks;
using Brokerage.Bilv1.Domain.Models.EnrolledBalances;

namespace Brokerage.Bilv1.Domain.Services
{
    public interface IWalletObservationService
    {
        Task RegisterWalletAsync(DepositWalletKey key);
        Task DeleteWalletAsync(DepositWalletKey key);
    }
}
