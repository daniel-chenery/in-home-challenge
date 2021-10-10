using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TT.Deliveries.Core.Configuration;
using TT.Deliveries.Business.Providers;
using TT.Deliveries.Core;
using TT.Deliveries.Core.Models;

namespace TT.Deliveries.Business.Services
{
    /// <summary>
    /// A simple implemenation of the Authentication service.
    /// A more realisitic implementation might connect to a database or other services
    /// </summary>
    public class SimpleAuthenticationService : IAuthenticationService
    {
        private readonly IJwtSecurityTokenHandlerFactory _jwtSecurityTokenHandlerFactory;
        private readonly IOptions<AuthenticationOptions> _options;

        public SimpleAuthenticationService(
            IJwtSecurityTokenHandlerFactory jwtSecurityTokenHandlerFactory,
            IOptions<AuthenticationOptions> options)
        {
            _jwtSecurityTokenHandlerFactory = jwtSecurityTokenHandlerFactory ?? throw new ArgumentNullException(nameof(jwtSecurityTokenHandlerFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public ValueTask<bool> CanTransition(ClaimsPrincipal identity, DeliveryState deliveryState)
        {
            var role = identity.Claims.Single(c => c.Type == ClaimTypes.Role)
                .Value
                .ToLowerInvariant();

            return new ValueTask<bool>((role, deliveryState) switch
            {
                ("user", DeliveryState.Approved) => true,
                ("partner", DeliveryState.Completed) => true,
                (_, DeliveryState.Cancelled) => true,
                (_, DeliveryState.Expired) => true,
                (_, _) => false
            });
        }

        public ValueTask<string> GenerateTokenAsync(params string[] roles)
        {
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r));

            var tokenHandler = _jwtSecurityTokenHandlerFactory.CreateJwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Value.Issuer,
                Audience = _options.Value.Audience,
                SigningCredentials = _options.Value.ToSigningCredentials(),
                Expires = Clock.UtcNow.AddDays(30).DateTime
            });

            return new ValueTask<string>(tokenHandler.WriteToken(token));
        }

        public ValueTask<bool> IsInRoleAsync(ClaimsPrincipal identity, string role) =>
            new ValueTask<bool>(identity.IsInRole(role) || identity.IsInRole(role.ToLowerInvariant()));
    }
}