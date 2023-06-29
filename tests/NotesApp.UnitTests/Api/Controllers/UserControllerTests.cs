using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Users;


namespace NotesApp.UnitTests.Api.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _usersController;
        private readonly Fixture _fixture;

        public UserControllerTests()
        {
            _fixture = new Fixture();
            _userServiceMock = new Mock<IUserService>();
            _usersController = new UserController(_userServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();
            var expectedUserDto = _fixture.Create<UserDto>();

            _userServiceMock.Setup(x => x.RegisterUserAsync(userRegisterDto))
                .ReturnsAsync(expectedUserDto);

            // Act
            var resultTask = _usersController.RegisterAsync(userRegisterDto);

            // Assert
            var result = await resultTask;
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expectedUserDto);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUserRegistrationFails()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();

            _userServiceMock.Setup(x => x.RegisterUserAsync(userRegisterDto))
                .Throws(new InvalidOperationException(ResponseMessages.EmailAlreadyExists));

            // Act
            Func<Task> act = () => _usersController.RegisterAsync(userRegisterDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(ResponseMessages.EmailAlreadyExists);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnOk_WhenLoginCredentialsAreValid()
        {
            // Arrange
            var userLoginDto = _fixture.Create<UserLoginDto>();
            var expectedUserDto = _fixture.Create<UserDto>();

            _userServiceMock.Setup(x => x.LoginUserAsync(userLoginDto))
                .ReturnsAsync(expectedUserDto);

            // Act
            var resultTask = _usersController.LoginAsync(userLoginDto);

            // Assert
            var result = await resultTask;
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expectedUserDto);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenLoginCredentialsAreInvalid()
        {
            // Arrange
            var userLoginDto = _fixture.Create<UserLoginDto>();

            _userServiceMock.Setup(x => x.LoginUserAsync(userLoginDto))
                .Throws(new Exception("Invalid password."));

            // Act
            Func<Task> act = () => _usersController.LoginAsync(userLoginDto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Invalid password.");
        }


    }

}
