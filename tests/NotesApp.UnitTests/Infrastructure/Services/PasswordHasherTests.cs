using AutoFixture;
using FluentAssertions;
using NotesApp.Infrastructure.Services;

namespace NotesApp.UnitTests.Infrastructure.Services
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IFixture _fixture;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
            _fixture = new Fixture();
        }

        [Fact]
        public void HashPassword_ShouldReturnNonEmptyAndUniqueHash_EvenIfPasswordIsSame()
        {
            // Arrange
            var password = _fixture.Create<string>();

            // Act
            var hashedPassword1 = _passwordHasher.HashPassword(password);
            var hashedPassword2 = _passwordHasher.HashPassword(password);

            // Assert
            hashedPassword1.Should().NotBeNullOrEmpty();
            hashedPassword1.Should().NotBeEquivalentTo(password);

            hashedPassword2.Should().NotBeNullOrEmpty();
            hashedPassword2.Should().NotBeEquivalentTo(password);

            hashedPassword1.Should().NotBeEquivalentTo(hashedPassword2);
        }


        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordIsCorrect()
        {
            // Arrange
            var password = _fixture.Create<string>();
            var hashedPassword = _passwordHasher.HashPassword(password);

            // Act
            var result = _passwordHasher.VerifyPassword(hashedPassword, password);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsWrong()
        {
            // Arrange
            var password = _fixture.Create<string>();
            var hashedPassword = _passwordHasher.HashPassword(password);
            var wrongPassword = _fixture.Create<string>();

            // Act
            var result = _passwordHasher.VerifyPassword(hashedPassword, wrongPassword);

            // Assert
            result.Should().BeFalse();
        }
    }

}
