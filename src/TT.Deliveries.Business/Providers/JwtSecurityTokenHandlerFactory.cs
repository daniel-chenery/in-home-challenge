using System.IdentityModel.Tokens.Jwt;

namespace TT.Deliveries.Business.Providers
{
    public class JwtSecurityTokenHandlerFactory : IJwtSecurityTokenHandlerFactory
    {
        public JwtSecurityTokenHandler CreateJwtSecurityTokenHandler() => new JwtSecurityTokenHandler();
    }
}