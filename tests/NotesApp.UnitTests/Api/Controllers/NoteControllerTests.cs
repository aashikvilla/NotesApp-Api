using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Notes;
using NotesApp.Common;
using NotesApp.Common.Models;


namespace NotesApp.UnitTests.Api.Controllers
{
    public class NoteControllerTests
    {
        private Mock<INoteService> _noteServiceMock;
        private NoteController _noteController;
        private Fixture _fixture;

        public NoteControllerTests()
        {
            _noteServiceMock = new Mock<INoteService>();
            _noteController = new NoteController(_noteServiceMock.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetNotesForUser_ShouldReturnNotes_WhenUserExists()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var notes = _fixture.CreateMany<NoteDto>(5);

            _noteServiceMock.Setup(s => s.GetNotesForUserAsync(userId)).ReturnsAsync(notes);

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId);

            // Assert
            _noteServiceMock.Verify(s => s.GetNotesForUserAsync(userId), Times.Once);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(notes);
        }

        [Fact]
        public async Task AddNote_ShouldReturnCreatedNote_WhenModelStateIsValid()
        {
            // Arrange
            var note = _fixture.Create<NoteDto>();

            _noteServiceMock.Setup(s => s.AddNoteAsync(note)).ReturnsAsync(note);

            // Act
            var result = await _noteController.AddNoteAsync(note);

            // Assert
            _noteServiceMock.Verify(s => s.AddNoteAsync(note), Times.Once);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(note);
        }

        [Fact]
        public async Task UpdateNote_ShouldReturnUpdatedNote_WhenUpdateIsSuccessful()
        {
            // Arrange
            var note = _fixture.Create<NoteDto>();

            _noteServiceMock.Setup(s => s.UpdateNoteAsync(note)).ReturnsAsync(note);

            // Act
            var result = await _noteController.UpdateNoteAsync(note);

            // Assert
            _noteServiceMock.Verify(s => s.UpdateNoteAsync(note), Times.Once);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(note);

        }

        [Fact]
        public async Task DeleteNote_ShouldReturnOk_WhenDeletionIsSuccessful()
        {
            // Arrange     
            var noteId = ObjectId.GenerateNewId().ToString();

            _noteServiceMock.Setup(s => s.DeleteNoteAsync(noteId));

            // Act
            var result = await _noteController.DeleteNoteAsync(noteId);

            // Assert
            _noteServiceMock.Verify(s => s.DeleteNoteAsync(noteId), Times.Once);
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenPageNumberIsInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = _fixture.Build<DataQueryParameters>().With(f => f.PageNumber, 0).Create();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(ResponseMessages.InvalidPageNumber);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = _fixture.Build<DataQueryParameters>().With(f => f.PageSize, 0).Create();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(ResponseMessages.InvalidPageSize);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var parameters = _fixture.Build<DataQueryParameters>().Create();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(ResponseMessages.InvalidUserId);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenSortByIsInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = _fixture.Build<DataQueryParameters>().Create();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(ResponseMessages.InvalidSortOrder);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenFilterParametersAreInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = _fixture.Build<DataQueryParameters>()
                .With(f => f.SortOrder, Constants.Ascending)
                .With(x => x.FilterColumns, _fixture.CreateMany<string>(1).ToArray())
                .With(x => x.FilterQueries, _fixture.CreateMany<string>(2).ToArray())
                .Create();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(ResponseMessages.InvalidFilterParameters);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnPaginationResult_WhenInputIsValid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = _fixture.Build<DataQueryParameters>()
                .With(f => f.SortOrder, Constants.Ascending)
                .With(x => x.FilterColumns, _fixture.CreateMany<string>(25).ToArray())
                .With(x => x.FilterQueries, _fixture.CreateMany<string>(25).ToArray())
                .Create();
            var expectedNotes = _fixture.Create<PaginationResult<NoteDto>>();
            _noteServiceMock.Setup(service => service.GetNotesForUserAsync(userId, parameters)).ReturnsAsync(expectedNotes);

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedNotes);

        }

    }
}
