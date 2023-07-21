using AutoFixture;
using FluentAssertions;
using Moq;
using NotesApp.Application.Common;
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
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Fixture _fixture = new();

        public UserServiceTests()
        {
            _userService = new UserService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();
            var existingUser = new User() { Email = userRegisterDto.Email };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(userRegisterDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _userService.RegisterUserAsync(userRegisterDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ResponseMessages.EmailAlreadyExists);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnUserDto_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(userRegisterDto.Email))
                .ReturnsAsync((User)null);

            _passwordHasherMock.Setup(x => x.HashPassword(userRegisterDto.Password))
                .Returns(_fixture.Create<string>());

            var expectedUserDto = new UserDto
            {
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                Email = userRegisterDto.Email
            };

            // Act
            var result = await _userService.RegisterUserAsync(userRegisterDto);

            // Assert
            result.Should().BeEquivalentTo(expectedUserDto);

            _userRepositoryMock.Verify(x => x.AddUserAsync(It.Is<User>(u =>
                u.FirstName == userRegisterDto.FirstName &&
                u.LastName == userRegisterDto.LastName &&
                u.Email == userRegisterDto.Email &&
                u.Password == _passwordHasherMock.Object.HashPassword(userRegisterDto.Password)
            )), Times.Once);
        }

      

        [Fact]
        public async Task LoginAsync_ShouldReturnUserDto_WhenCredentialsAreValid()
        {
            // Arrange
            var loginUserDto = _fixture.Create<UserLoginDto>();
            var user = _fixture.Build<User>().Create();
            var jwtToken = _fixture.Create<string>();
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = jwtToken
            };

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(loginUserDto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.VerifyPassword(user.Password, loginUserDto.Password))
                .Returns(true);

            _tokenServiceMock.Setup(x => x.GenerateToken(user))
                .Returns(jwtToken);

            userDto.Token = jwtToken;

            // Act
            var result = await _userService.LoginUserAsync(loginUserDto);

            // Assert
            result.Should().BeEquivalentTo(userDto);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var loginUserDto = _fixture.Create<UserLoginDto>();

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(loginUserDto.Email))
                .ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _userService.LoginUserAsync(loginUserDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage(ResponseMessages.EmailNotFound);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsNotValid()
        {
            // Arrange
            var loginUserDto = _fixture.Create<UserLoginDto>();
            var user = _fixture.Build<User>().Create();

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(loginUserDto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.VerifyPassword(user.Password, loginUserDto.Password))
                .Returns(false);

            // Act
            Func<Task> act = async () => await _userService.LoginUserAsync(loginUserDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage(ResponseMessages.InvalidPassword);
        }


    }

}
