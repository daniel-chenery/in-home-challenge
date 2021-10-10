using System.IdentityModel.Tokens.Jwt;

namespace TT.Deliveries.Business.Providers
{
    public interface IJwtSecurityTokenHandlerFactory
    {
        JwtSecurityTokenHandler CreateJwtSecurityTokenHandler();
    }
}