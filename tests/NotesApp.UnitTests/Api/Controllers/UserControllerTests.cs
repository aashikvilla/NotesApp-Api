using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Users;
using NotesApp.Application.Validators.Users;

namespace NotesApp.UnitTests.Api.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _usersController;
        private readonly Fixture _fixture;
        private readonly IValidator<UserRegisterDto> _userRegisterValidator;
        private readonly IValidator<UserLoginDto> _userLoginValidator;

        public UserControllerTests()
        {
            _fixture = new Fixture();
            _userServiceMock = new Mock<IUserService>();
            _userRegisterValidator = new UserRegisterDtoValidator();
            _userLoginValidator = new UserLoginDtoValidator();
            _usersController = new UserController(_userServiceMock.Object, _userRegisterValidator, _userLoginValidator);
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
        public async Task RegisterAsync_ShouldBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var userRegisterDto = new UserRegisterDto();

            var expectedErrors = new string[]
            {
                ResponseMessages.EmailRequired,
                ResponseMessages.PasswordRequired,
                ResponseMessages.FirstNameRequired,
                ResponseMessages.LastNameRequired
            };

            // Act
            var result = await _usersController.RegisterAsync(userRegisterDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var errors = (result as BadRequestObjectResult).Value as IEnumerable<string>;
            errors.Should().BeEquivalentTo(expectedErrors);
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
        public async Task LoginAsync_ShouldBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var userLoginDto = new UserLoginDto();

            var expectedErrors = new string[]
            {
                ResponseMessages.EmailRequired,
                ResponseMessages.PasswordRequired
            };

            // Act
            var result = await _usersController.LoginAsync(userLoginDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var errors = (result as BadRequestObjectResult).Value as IEnumerable<string>;
            errors.Should().BeEquivalentTo(expectedErrors);

        }


    }

}
