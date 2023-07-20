using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using NotesApp.IntegrationTests.Helpers;
using NotesApp.IntegrationTests.TestConstants;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using NotesApp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using NotesApp.Application.Common;
using System.Net.Http.Headers;
using NotesApp.Infrastructure.Services;
using MongoDB.Driver;
using MongoDB.Bson;

namespace NotesApp.IntegrationTests.Controllers
{
    public class NoteControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly IFixture _fixture;
        private readonly ITokenService _tokenService;

        public NoteControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _fixture = new Fixture();

            using (var scope = _factory.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                _tokenService = services.GetRequiredService<ITokenService>();
            }
  
        }

        private string GetToken()
        {
            var userFromSeedData = Utilities.GetSeedingUsers().FirstOrDefault();           
            return _tokenService.GenerateToken(userFromSeedData);
        }
           

        [Fact]
        public async Task GetNotesForUser_ShouldReturnUserNotes_WhenUserIsValid()
        {
            // Arrange
            Utilities.ReinitializeDb(_factory);
            var userFromSeedData = Utilities.GetSeedingUsers().FirstOrDefault();
            var userNotesFromSeedData = Utilities.GetSeedingNotes().Where(n => n.UserId == userFromSeedData.Id);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.GetAsync( string.Format(UrlRouteConstants.GetNotesForUser, userFromSeedData.Id) );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var notes = await response.Content.ReadFromJsonAsync<IEnumerable<Note>>();

            //check if id is auto generated
           notes.Should().BeEquivalentTo(userNotesFromSeedData);
        }

        [Fact]
        public async Task GetNotesForUser_ShouldReturnBadRequest_WhenUserIsInvalid()
        {
            // Arrange
            int invalidId = 0;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.GetAsync(string.Format(UrlRouteConstants.GetNotesForUser, invalidId));

            // Assert
           
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var message = await response.Content.ReadAsStringAsync();
            message.Should().Contain(ResponseMessages.InvalidUserId);
        }

        [Fact]
        public async Task GetNotesForUser_ShouldReturnBadRequest_WhenUserIsNotPresent()
        {
            // Arrange
            Utilities.ReinitializeDb(_factory);
            
            string invalidId = ObjectId.GenerateNewId().ToString();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.GetAsync(string.Format(UrlRouteConstants.GetNotesForUser, invalidId));

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var message = await response.Content.ReadAsStringAsync();
            message.Should().Contain(ResponseMessages.UserNotFound);
        }

        [Fact]
        public async Task GetNotesForUser_ShouldReturnUnauthorized_WhenTokenIsNotPresent()
        {
            // Arrange
            var userId = _fixture.Create<int>();

            // Act
            var response = await _client.GetAsync(string.Format(UrlRouteConstants.GetNotesForUser, userId));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddNote_ShouldReturnNote_WhenNoteIsValid()
        {
            // Arrange         
            var userFromSeedData = Utilities.GetSeedingUsers().FirstOrDefault();
            var note = _fixture.Build<Note>().With(n => n.Id, string.Empty).With(n => n.UserId, userFromSeedData.Id).Create();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.AddNote, note);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var addedNote = await response.Content.ReadFromJsonAsync<Note>();

            //check if id is auto generated
            addedNote.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddNote_ShouldReturnBadRequest_WhenNoteIsInvalid()
        {
            // Arrange  
            var note = Utilities.GetSeedingNotes().FirstOrDefault();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.AddNote, note);

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var message = await response.Content.ReadAsStringAsync();
            message.Should().Contain(ResponseMessages.NoteAlreadyExists);
        }

        [Fact]
        public async Task AddNote_ShouldReturnUnauthorized_WhenTokenIsNotPresent()
        {
            // Arrange
            var note = _fixture.Create<Note>();

            // Act
            var response = await _client.PostAsJsonAsync(UrlRouteConstants.AddNote, note);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateNote_ShouldReturnNote_WhenNoteIsValid()
        {
            // Arrange
            Utilities.ReinitializeDb(_factory);
            var note = Utilities.GetSeedingNotes().FirstOrDefault();
            var title = _fixture.Create<string>();
            note.Title = title;
            
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.PutAsJsonAsync(UrlRouteConstants.UpdateNote, note);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedNote = await response.Content.ReadFromJsonAsync<Note>();

            //check if title is updated
            updatedNote.Title.Should().BeEquivalentTo(title);
        }

        [Fact]
        public async Task UpdateNote_ShouldReturnBadRequest_WhenNoteIsInvalid()
        {
            // Arrange
            var note = Utilities.GetSeedingNotes().FirstOrDefault();
            note.Id = ObjectId.GenerateNewId().ToString();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.PutAsJsonAsync(UrlRouteConstants.UpdateNote, note);

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var message = await response.Content.ReadAsStringAsync();
            message.Should().Contain(ResponseMessages.NoteNotFound);
        }

        [Fact]
        public async Task UpdateNote_ShouldReturnUnauthorized_WhenTokenIsNotPresent()
        {
            // Arrange
            var note = _fixture.Create<Note>();

            // Act
            var response = await _client.PutAsJsonAsync(UrlRouteConstants.UpdateNote, note);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteNote_ShouldReturnOk_WhenNoteIsValid()
        {
            // Arrange
            Utilities.ReinitializeDb(_factory);
            var note = Utilities.GetSeedingNotes().FirstOrDefault();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.DeleteAsync( string.Format( UrlRouteConstants.DeleteNote,note.Id) );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
    
        }

        [Fact]
        public async Task DeleteNote_ShouldReturnBadRequest_WhenNoteIsInvalid()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            var invalidId = _fixture.Create<string>();

            // Act
            var response = await _client.DeleteAsync(string.Format(UrlRouteConstants.DeleteNote, invalidId));

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var message = await response.Content.ReadAsStringAsync();
            message.Should().Contain(ResponseMessages.InvalidNoteId);
        }

        [Fact]
        public async Task DeleteNote_ShouldReturnBadRequest_WhenNoteIsNotPresent()
        {
            // Arrange
            Utilities.ReinitializeDb(_factory);                  
            string noteId = ObjectId.GenerateNewId().ToString();         

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());

            // Act
            var response = await _client.DeleteAsync(string.Format(UrlRouteConstants.DeleteNote, noteId));

            // Assert

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var message = await response.Content.ReadAsStringAsync();
            message.Should().Contain(ResponseMessages.NoteNotFound);
        }

        [Fact]
        public async Task DeleteNote_ShouldReturnUnauthorized_WhenTokenIsNotPresent()
        {
            // Arrange
            var noteId = _fixture.Create<int>();

            // Act
            var response = await _client.DeleteAsync(string.Format(UrlRouteConstants.DeleteNote, noteId));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
