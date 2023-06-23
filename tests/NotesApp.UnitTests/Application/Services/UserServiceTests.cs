using AutoFixture;
using FluentAssertions;
using Moq;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Users;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Services;

namespace NotesApp.UnitTests.Application.Services
{
    public class UserServiceTests
    {
        private readonly UserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Fixture _fixture = new();

        public UserServiceTests()
        {
            _userService = new UserService(_userRepositoryMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();
            var existingUser = new User() { Email = userRegisterDto.Email };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(userRegisterDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.RegisterUserAsync(userRegisterDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Email already exists.");
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnSuccess_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(userRegisterDto.Email))
                .ReturnsAsync((User)null);

            _passwordHasherMock.Setup(x => x.HashPassword(userRegisterDto.Password))
                .Returns(_fixture.Create<string>());

            // Act
            var result = await _userService.RegisterUserAsync(userRegisterDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.FirstName.Should().Be(userRegisterDto.FirstName);
            result.Data.LastName.Should().Be(userRegisterDto.LastName);
            result.Data.Email.Should().Be(userRegisterDto.Email);
            result.Message.Should().Be("User registered successfully.");

            _userRepositoryMock.Verify(x => x.AddUserAsync(It.Is<User>(u =>
                u.FirstName == userRegisterDto.FirstName &&
                u.LastName == userRegisterDto.LastName &&
                u.Email == userRegisterDto.Email &&
                u.Password == _passwordHasherMock.Object.HashPassword(userRegisterDto.Password)
            )), Times.Once);
        }
    }

}
