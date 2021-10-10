using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TT.Deliveries.Business.Providers
{
    public class JwtSecurityTokenHandlerFactory : IJwtSecurityTokenHandlerFactory
    {
        public JwtSecurityTokenHandler CreateJwtSecurityTokenHandler() => new JwtSecurityTokenHandler();
    }
}