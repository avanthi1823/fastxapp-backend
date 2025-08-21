using NUnit.Framework;
using NewFastBus.Services;
using NewFastBus.Models.Entities;
using Microsoft.Extensions.Configuration;

namespace NewFastBus.Tests
{
    [TestFixture]
    public class TokenServiceTests
    {
        private TokenService _tokenService;

        [SetUp]
        public void Setup()
        {
            
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "ThisIsASecretKeyForJwtTokenTesting123!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenService = new TokenService(configuration);
        }

        [Test]
        public void GenerateToken_ValidUser_ReturnsToken()
        {
            // Arrange
            var user = new UserMaster
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@example.com"
            };
            var roleName = "Admin";

            // Act
            var token = _tokenService.GenerateToken(user, roleName);

            // Assert
            Assert.IsNotNull(token, "Token should not be null.");
            Assert.IsNotEmpty(token, "Token should not be empty.");
            TestContext.WriteLine("Generated Token: " + token);
        }
    }
}
