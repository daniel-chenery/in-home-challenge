using System.Security.Claims;
using System.Threading.Tasks;
using TT.Deliveries.Core.Models;

namespace TT.Deliveries.Business.Services
{
    public interface IAuthenticationService
    {
        public ValueTask<string> GenerateTokenAsync(params string[] roles);

        public ValueTask<bool> IsInRoleAsync(ClaimsPrincipal identity, string role);

        public ValueTask<bool> CanTransition(ClaimsPrincipal identity, DeliveryState deliveryState);
    }
}