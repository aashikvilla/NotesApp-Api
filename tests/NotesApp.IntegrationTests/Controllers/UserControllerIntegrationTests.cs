using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Response;
using NotesApp.Infrastructure.Data;
using NotesApp.IntegrationTests.Helpers;
using NotesApp.IntegrationTests.TestConstants;
using System.Net;
using System.Net.Http.Json;

namespace NotesApp.IntegrationTests.Controllers
{
    public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly IFixture _fixture;

        public UserControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _fixture = new Fixture();
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenUserRegistrationIsValid()
        {
            // Arrange
            var userRegisterDto = _fixture.Create<UserRegisterDto>();         

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.Register, userRegisterDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();
            serviceResponse.Success.Should().BeTrue();
            //check if id is auto generated
            serviceResponse.Data.Id.Should().BeGreaterThan(0);
            serviceResponse.Data.Email.Should().BeEquivalentTo(userRegisterDto.Email);
        }


        [Fact]
        public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var userRegisterDto = _fixture.Build<UserRegisterDto>()
                .With(x => x.Email, Utilities.validUserLogin.Email)
                .Create();

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.Register, userRegisterDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();
            serviceResponse.Success.Should().BeFalse();
            serviceResponse.Message.Should().BeEquivalentTo(ResponseMessages.EmailAlreadyExists);
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var userLoginDto = Utilities.validUserLogin;
            var userDetailsFromSeedData = Utilities.GetSeedingUsers().First(u => u.Email == userLoginDto.Email);

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.Login, userLoginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();
            serviceResponse.Success.Should().BeTrue();
            serviceResponse.Message.Should().BeEquivalentTo(ResponseMessages.LoginSuccessful);
            //check if token is generated
            serviceResponse.Data.Token.Should().NotBeNullOrEmpty();
            //check if data matches seed data
            serviceResponse.Data.FirstName.Should().BeEquivalentTo(userDetailsFromSeedData.FirstName);
            serviceResponse.Data.LastName.Should().BeEquivalentTo(userDetailsFromSeedData.LastName);

        }


        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsIncorrect()
        {
            // Arrange
            var userLoginDto = new UserLoginDto
            {
                Email = Utilities.validUserLogin.Email,
                Password = "IncorrectPassword"  
            };

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.Login, userLoginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();
            serviceResponse.Success.Should().BeFalse();
            serviceResponse.Message.Should().BeEquivalentTo(ResponseMessages.InvalidPassword);
        }


        [Fact]
        public async Task LoginAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                Utilities.ReinitializeDbForTests(db);
            }
            var userLoginDto = _fixture.Create<UserLoginDto>();               

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.Login, userLoginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();
            serviceResponse.Success.Should().BeFalse();
            serviceResponse.Message.Should().BeEquivalentTo(ResponseMessages.EmailNotFound);
        }

    }
}
