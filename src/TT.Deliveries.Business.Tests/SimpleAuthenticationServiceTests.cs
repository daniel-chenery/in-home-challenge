using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TT.Deliveries.Business.Providers;
using TT.Deliveries.Business.Services;
using TT.Deliveries.Core;
using TT.Deliveries.Core.Configuration;

namespace TT.Deliveries.Business.Tests
{
    [TestFixture]
    internal class SimpleAuthenticationServiceTests
    {
        private const string Audience = "UnitTest" + nameof(Audience);
        private const string Issuer = "UnitTest" + nameof(Issuer);
        private const string Secret = "UnitTest" + nameof(Secret);

        private Mock<JwtSecurityTokenHandler> _mockSecurityTokenHandler;
        private AuthenticationOptions _options;

        [SetUp]
        public void Setup()
        {
            _mockSecurityTokenHandler = new Mock<JwtSecurityTokenHandler>();
            _options = new AuthenticationOptions
            {
                Audience = Audience,
                Issuer = Issuer,
                Secret = Secret
            };
        }

        [TestCase]
        [TestCase("UnitTest")]
        [TestCase("Unit", "Test")]
        public async Task GenerateTokenAsync_PopulatesTokenDescriptor(params string[] roles)
        {
            // arrange
            var service = GetService();
            var claims = new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var credentials = _options.ToSigningCredentials();
            var now = new DateTimeOffset(new DateTime(1999, 01, 01), TimeSpan.Zero);
            Clock.SetUtc(() => now);
            SecurityTokenDescriptor? descriptor = default;

            _mockSecurityTokenHandler
                .Setup(sth => sth.CreateToken(It.IsNotNull<SecurityTokenDescriptor>()))
                .Callback<SecurityTokenDescriptor>(std => descriptor = std);

            // act
            _ = await service.GenerateTokenAsync(roles);

            // assert
            Assert.That(descriptor, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(descriptor.Issuer, Is.EqualTo(Issuer));
                Assert.That(descriptor.Audience, Is.EqualTo(Audience));
                Assert.That(descriptor.Expires, Is.EqualTo(now.AddDays(30).DateTime));
                Assert.That(descriptor.Subject.Claims, Has.Exactly(roles.Length).Items
                    .With.Property(nameof(Claim.Type)).EqualTo(ClaimTypes.Role));
                Assert.That(descriptor.SigningCredentials.Key.KeyId, Is.EqualTo(credentials.Key.KeyId));
                Assert.That(descriptor.SigningCredentials.Algorithm, Is.EqualTo(credentials.Algorithm));
            });
        }

        [Test]
        public async Task GenerateTokenAsync_Returns_SecurityToken()
        {
            // arrange
            var service = GetService();
            var expectedToken = "Unit Test Token";

            _mockSecurityTokenHandler
                .Setup(sth => sth.WriteToken(It.IsAny<SecurityToken>()))
                .Returns(expectedToken);

            // act
            var token = await service.GenerateTokenAsync(Array.Empty<string>());

            // assert
            Assert.That(token, Is.SameAs(expectedToken));
        }

        [Test]
        public async Task IsInRole_Async_Returns_True_For_Valid_Role()
        {
            // arrange
            const string role = nameof(role);
            var identity = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }));
            var service = GetService();

            // act
            var result = await service.IsInRoleAsync(identity, role);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsInRole_Async_Returns_False_For_Inalid_Role()
        {
            // arrange
            const string role = nameof(role);
            var identity = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }));
            var service = GetService();

            // act
            var result = await service.IsInRoleAsync(identity, "AnotherRole");

            // assert
            Assert.That(result, Is.False);
        }

        private IAuthenticationService GetService()
        {
            var mockFactory = Mock.Of<IJwtSecurityTokenHandlerFactory>(f => f.CreateJwtSecurityTokenHandler() == _mockSecurityTokenHandler.Object);
            var mockOptions = Mock.Of<IOptions<AuthenticationOptions>>(o => o.Value == _options);

            return new SimpleAuthenticationService(mockFactory, mockOptions);
        }
    }
}