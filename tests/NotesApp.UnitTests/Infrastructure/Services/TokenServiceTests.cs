using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Configuration;
using NotesApp.Domain.Entities;
using NotesApp.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NotesApp.UnitTests.Infrastructure.Services
{
    public class TokenServiceTests
    {
        private ITokenService _tokenService;
        private IConfiguration _configuration;
        private IFixture _fixture;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "YourSuperSecureKey"} 
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenService = new TokenService(_configuration);
            _fixture = new Fixture();
        }
        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken_ForValidUser()
        {
            // Arrange
            var user = _fixture.Create<User>();
             

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Claims.Should().NotBeEmpty();
            var expectedTokenclaim = new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());

            var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            idClaim.Should().NotBeNull();
            idClaim.Value.Should().Be(user.Id.ToString());

            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name");
            emailClaim.Should().NotBeNull();
            emailClaim.Value.Should().Be(user.Email);

            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), 1.Seconds());  // Allow 1 second difference
        }

    }

}
