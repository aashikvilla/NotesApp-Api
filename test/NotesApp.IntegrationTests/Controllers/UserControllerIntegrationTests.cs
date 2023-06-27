using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Response;
using NotesApp.IntegrationTests.Helpers;
using NotesApp.IntegrationTests.TestConstants;
using System.Net;
using System.Net.Http.Json;
using System.Text;

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
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.Register, userRegisterDto);
      

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();
            serviceResponse.Should().BeEquivalentTo(expectedResult);
        }




        [Fact]
        public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var userLoginDto = Utilities.validUserLogin;

            var userDetailsFromSeedData = Utilities.GetSeedingUsers().First(u => u.Email == userLoginDto.Email);

            var jsonString = JsonConvert.SerializeObject(userLoginDto);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(UrlRouteConstants.Login, httpContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<UserDto>>();     
            serviceResponse.Success.Should().BeTrue();
            serviceResponse.Message.Should().BeEquivalentTo(ResponseMessages.LoginSuccessful);
            serviceResponse.Data.Token.Should().NotBeNullOrEmpty();
            serviceResponse.Data.FirstName.Should().BeEquivalentTo(userDetailsFromSeedData.FirstName);
            serviceResponse.Data.LastName.Should().BeEquivalentTo(userDetailsFromSeedData.LastName);

        }


    }
}
