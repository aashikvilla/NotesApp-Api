using AutoFixture;
using FluentAssertions;
using Moq;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Response;
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
            result.Message.Should().Be(ResponseMessages.EmailAlreadyExists);
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

            var expectedUserDto = new UserDto
            {
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                Email = userRegisterDto.Email
            };

            var expectedResult = new ServiceResponse<UserDto>
            {
                Data = expectedUserDto,
                Success = true,
                Message = ResponseMessages.RegistrationSuccessful
            };

            // Act
            var result = await _userService.RegisterUserAsync(userRegisterDto);

            // Assert

            result.Should().BeEquivalentTo(expectedResult);

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
            var user = _fixture.Build<User>().Without(u => u.Notes).Create();
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

            var expectedResult = new ServiceResponse<UserDto>
            {
                Data = userDto,
                Success = true,
                Message =ResponseMessages.LoginSuccessful
            };

            // Act
            var result = await _userService.LoginUserAsync(loginUserDto);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNullData_WhenUserDoesNotExist()
        {
            // Arrange
            var loginUserDto = _fixture.Create<UserLoginDto>();

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(loginUserDto.Email))
                .ReturnsAsync((User)null);

            var expectedResult = new ServiceResponse<UserDto>
            { 
                Message =ResponseMessages.EmailNotFound
            };

            // Act
            var result = await _userService.LoginUserAsync(loginUserDto);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNullData_WhenPasswordIsNotValid()
        {
            // Arrange
            var loginUserDto = _fixture.Create<UserLoginDto>();
            var user = _fixture.Build<User>().Without(u => u.Notes).Create();

            _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(loginUserDto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock.Setup(x => x.VerifyPassword(user.Password, loginUserDto.Password))
                .Returns(false);

            var expectedResult = new ServiceResponse<UserDto>
            {
                Message =ResponseMessages.InvalidPassword
            };

            // Act
            var result = await _userService.LoginUserAsync(loginUserDto);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

    }

}
