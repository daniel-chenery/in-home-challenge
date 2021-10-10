using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace TT.Deliveries.Core.Configuration
{
    public class AuthenticationOptions
    {
        public const string AppSettingsSection = "Authentication";

        public string? Secret { get; set; }

        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public SymmetricSecurityKey ToSecurityKey()
        {
            if (string.IsNullOrWhiteSpace(Secret))
            {
                throw new ArgumentException($"The {nameof(Secret)} must not be null or empty.", nameof(Secret));
            }

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        }

        public SigningCredentials ToSigningCredentials() => new SigningCredentials(ToSecurityKey(), SecurityAlgorithms.HmacSha256Signature);
    }
}