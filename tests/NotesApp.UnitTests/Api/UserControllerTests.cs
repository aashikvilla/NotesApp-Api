using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Application.Dto;
using NotesApp.Application.Response;
using NotesApp.Application.Services.User;


namespace NotesApp.UnitTests.Api
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
            var serviceResponse = new ServiceResponse<UserDto>
            {
                Data = _fixture.Create<UserDto>(),
                Success = true,
                Message = "Registration successful."
            };

            _userServiceMock.Setup(x => x.RegisterUserAsync(userRegisterDto))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _usersController.RegisterAsync(userRegisterDto);

            // Assert
   
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(serviceResponse);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnBadRequest_WhenUserRegistrationFails()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();
            var serviceResponse = new ServiceResponse<UserDto>
            {
                Success = false,
                Message = "Registration failed."
            };

            _userServiceMock.Setup(x => x.RegisterUserAsync(userRegisterDto))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _usersController.RegisterAsync(userRegisterDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeEquivalentTo(serviceResponse);
        }

    }

}
