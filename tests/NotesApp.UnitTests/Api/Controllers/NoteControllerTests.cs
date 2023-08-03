using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using NotesApp.Api.Controllers;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Notes;
using NotesApp.Application.Validators.Common;
using NotesApp.Application.Validators.Notes;
using NotesApp.Common;
using NotesApp.Common.Models;
using NotesApp.Domain.Entities;

namespace NotesApp.UnitTests.Api.Controllers
{
    public class NoteControllerTests
    {
        private Mock<INoteService> _noteServiceMock;
        private readonly IValidator<DataQueryParameters> _dataQueryParametersValidator;
        private readonly IValidator<NoteDto> _noteDtoValidator;

        private NoteController _noteController;
        private Fixture _fixture;

        public NoteControllerTests()
        {
            _noteServiceMock = new Mock<INoteService>();
            _dataQueryParametersValidator = new DataQueryParametersValidator();
            _noteDtoValidator = new NoteDtoValidator();
            _noteController = new NoteController(_noteServiceMock.Object, _noteDtoValidator, _dataQueryParametersValidator);
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
            var note = _fixture.Build<NoteDto>()
                .With(n => n.Id, string.Empty)
                .With(n => n.UserId, ObjectId.GenerateNewId().ToString())
                .Create();


            _noteServiceMock.Setup(s => s.AddNoteAsync(note)).ReturnsAsync(note);

            // Act
            var result = await _noteController.AddNoteAsync(note);

            // Assert
            _noteServiceMock.Verify(s => s.AddNoteAsync(note), Times.Once);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(note);
        }

        [Fact]
        public async Task AddNote_ShouldReturnBadRequest_WhenModelStateIsInValid()
        {
            // Arrange
            var note = new NoteDto
            {
                Id = ObjectId.GenerateNewId().ToString()
            };
            var expectedErrors = new string[]
            {
                ResponseMessages.InvalidUserId,
                ResponseMessages.InvalidNoteId,
                ResponseMessages.TitleRequired,
                ResponseMessages.DescriptionRequired,
                ResponseMessages.PriorityRequired,
                ResponseMessages.StatusRequired,
                ResponseMessages.UserIdRequired
             };

            // Act
            var result = await _noteController.AddNoteAsync(note);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var errors = (result as BadRequestObjectResult).Value as IEnumerable<string>;
            errors.Should().BeEquivalentTo(expectedErrors);
        }

        [Fact]
        public async Task UpdateNote_ShouldReturnUpdatedNote_WhenUpdateIsSuccessful()
        {
            // Arrange
            var note = _fixture.Build<NoteDto>()
                .With(n => n.Id, ObjectId.GenerateNewId().ToString())
                .With(n => n.UserId, ObjectId.GenerateNewId().ToString())
                .Create();

            _noteServiceMock.Setup(s => s.UpdateNoteAsync(note)).ReturnsAsync(note);

            // Act
            var result = await _noteController.UpdateNoteAsync(note);

            // Assert
            _noteServiceMock.Verify(s => s.UpdateNoteAsync(note), Times.Once);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(note);

        }

        [Fact]
        public async Task UpdateNote_ShouldReturnBadRequest_WhenModelStateIsInValid()
        {
            // Arrange
            var note = new NoteDto
            {
                Id = string.Empty
            };
            var expectedErrors = new string[]
            {
                ResponseMessages.InvalidUserId,
                ResponseMessages.InvalidNoteId,
                ResponseMessages.TitleRequired,
                ResponseMessages.DescriptionRequired,
                ResponseMessages.PriorityRequired,
                ResponseMessages.StatusRequired,
                ResponseMessages.UserIdRequired
            };

            // Act
            var result = await _noteController.UpdateNoteAsync(note);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var errors = (result as BadRequestObjectResult).Value as IEnumerable<string>;
            errors.Should().BeEquivalentTo(expectedErrors);
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
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should()
                .BeAssignableTo<IEnumerable<string>>().Which.Should().Contain(ResponseMessages.InvalidPageNumber);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = new DataQueryParameters
            {
                PageSize = 0
            };

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should()
                .BeAssignableTo<IEnumerable<string>>().Which.Should().Contain(ResponseMessages.InvalidPageSize);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var parameters = new DataQueryParameters();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be(ResponseMessages.InvalidUserId);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenSortOrderIsInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = _fixture.Build<DataQueryParameters>().Create();

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should()
                .BeAssignableTo<IEnumerable<string>>().Which.Should().Contain(ResponseMessages.InvalidSortOrder);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenFilterParametersAreInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = new DataQueryParameters()
            {
                FilterQueries = _fixture.CreateMany<string>(2).ToArray()
            };

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should()
                .BeAssignableTo<IEnumerable<string>>().Which.Should().Contain(ResponseMessages.InvalidFilterParameters);

        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnPaginationResult_WhenInputIsValid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var parameters = new DataQueryParameters()
            {
                SortBy = nameof(Note.Title),
                SortOrder = Constants.Ascending,
                FilterColumns = new string[] { nameof(Note.Title), nameof(Note.Description) },
                FilterQueries = _fixture.CreateMany<string>(2).ToArray()
            };
            var expectedNotes = _fixture.Create<PaginationResult<NoteDto>>();
            _noteServiceMock.Setup(service => service.GetNotesForUserAsync(userId, parameters)).ReturnsAsync(expectedNotes);

            // Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedNotes);

        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenFilterColumnIsInvalid()
        {
            // Arrange          
            var userId = ObjectId.GenerateNewId().ToString();
            var invalidColumn = _fixture.Create<string>();
            var parameters = _fixture.Build<DataQueryParameters>()
                .With(p => p.SortBy, nameof(Note.Description))
                .With(p => p.FilterColumns, new string[] { invalidColumn })
                .Create();

            //Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should().BeAssignableTo<IEnumerable<string>>()
                .Which.Should().Contain(string.Format(ResponseMessages.InvalidFilterColumn, invalidColumn));

        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnBadRequest_WhenSortByIsInvalid()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var invalidColumn = _fixture.Create<string>();
            var parameters = _fixture.Build<DataQueryParameters>()
               .With(p => p.SortBy, invalidColumn)
               .With(p => p.FilterColumns, new string[] { nameof(Note.Title) })
               .Create();

            //Act
            var result = await _noteController.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult).Value.Should().BeAssignableTo<IEnumerable<string>>()
                .Which.Should().Contain(string.Format(ResponseMessages.InvalidSortByColumn, invalidColumn));

        }

    }
}
