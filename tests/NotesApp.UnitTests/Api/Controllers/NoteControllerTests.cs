using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Application.Services.Notes;
using NotesApp.Domain.Entities;


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
            var userId = _fixture.Create<int>();
            var notes = _fixture.CreateMany<Note>(5);

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
            var note = _fixture.Create<Note>();

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
            var note = _fixture.Create<Note>();

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
            var noteId = _fixture.Create<int>();

            _noteServiceMock.Setup(s => s.DeleteNoteAsync(noteId)).ReturnsAsync(true);

            // Act
            var result = await _noteController.DeleteNoteAsync(noteId);

            // Assert
            _noteServiceMock.Verify(s => s.DeleteNoteAsync(noteId), Times.Once);
            result.Should().BeOfType<OkResult>();
        }
    }
}
