using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Moq;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Profiles;
using NotesApp.Application.Services.Notes;
using NotesApp.Common.Models;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;

namespace NotesApp.UnitTests.Application.Services
{
    public class NoteServiceTests
    {
        private readonly Mock<INoteRepository> _noteRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly IMapper _mapper;
        private readonly INoteService _noteService;
        private readonly Fixture _fixture;

        public NoteServiceTests()
        {
            _fixture = new Fixture();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<NoteProfile>();
            });

            _mapper = config.CreateMapper();
            _noteRepositoryMock = new Mock<INoteRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _noteService = new NoteService(_noteRepositoryMock.Object, _userRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnNotes_WhenUserExists()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var notes = _fixture.CreateMany<Note>(5);

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new User());
            _noteRepositoryMock.Setup(r => r.GetNotesForUserAsync(userId)).ReturnsAsync(notes);

            // Act
            var result = await _noteService.GetNotesForUserAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(notes);

            // Verify that GetNotesForUserAsync was called once with the correct parameter
            _noteRepositoryMock.Verify(r => r.GetNotesForUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            _userRepositoryMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _noteService.GetNotesForUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ResponseMessages.UserNotFound);
        }

        [Fact]
        public async Task AddNoteAsync_ShouldReturnNote_WhenNoteIsAddedSuccessfully()
        {
            // Arrange
            var noteDto = _fixture.Build<NoteDto>().With(n => n.Id, string.Empty).Create();
            var note = _fixture.Build<Note>()
                .With(n => n.Id, noteDto.Id)
                .With(n => n.Title, noteDto.Title)
                .With(n => n.Description, noteDto.Description)
                .With(n => n.Status, noteDto.Status)
                .With(n => n.Priority, noteDto.Priority)
                .Create();

            _noteRepositoryMock.Setup(x => x.GetNoteByIdAsync(note.Id)).ReturnsAsync(note);

            // Act
            var result = await _noteService.AddNoteAsync(noteDto);

            // Assert
            _noteRepositoryMock.Verify(r => r.AddNoteAsync(note), Times.Once);
            result.Should().BeEquivalentTo(note);

        }

        [Fact]
        public async Task AddNoteAsync_ShouldThrowException_WhenNoteAlreadyExists()
        {
            // Arrange
            var note = _fixture.Create<NoteDto>();

            // Act
            Func<Task> act = async () => await _noteService.AddNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ResponseMessages.NoteAlreadyExists);
        }

        [Fact]
        public async Task UpdateNoteAsync_ShouldReturnNote_WhenNoteIsUpdatedSuccessfully()
        {
            // Arrange
            var noteDto = _fixture.Create<NoteDto>();
            var note = _fixture.Build<Note>()
               .With(n => n.Id, noteDto.Id)
               .With(n => n.Title, noteDto.Title)
               .With(n => n.Description, noteDto.Description)
               .With(n => n.Status, noteDto.Status)
               .With(n => n.Priority, noteDto.Priority)
               .Create();

            _noteRepositoryMock.Setup(x => x.GetNoteByIdAsync(note.Id)).ReturnsAsync(note);

            // Act
            var result = await _noteService.UpdateNoteAsync(noteDto);

            // Assert
            _noteRepositoryMock.Verify(r => r.UpdateNoteAsync(note), Times.Once);
            result.Should().BeEquivalentTo(note);

        }

        [Fact]
        public async Task UpdateNoteAsync_ShouldThrowException_WhenNoteDoesNotExist()
        {
            // Arrange
            var note = _fixture.Create<NoteDto>();

            _noteRepositoryMock.Setup(x => x.GetNoteByIdAsync(note.Id))
                .ReturnsAsync((Note)null);

            // Act
            Func<Task> act = async () => await _noteService.UpdateNoteAsync(note);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ResponseMessages.NoteNotFound);
        }

        [Fact]
        public async Task DeleteNoteAsync_ShouldNotThrowException_WhenNoteIsDeletedSuccessfully()
        {
            // Arrange
            var noteId = _fixture.Create<string>();
            _noteRepositoryMock.Setup(x => x.GetNoteByIdAsync(noteId)).ReturnsAsync(new Note());

            // Act
            Func<Task> act = async () => { await _noteService.DeleteNoteAsync(noteId); };

            // Assert
            await act.Should().NotThrowAsync();
        }


        [Fact]
        public async Task DeleteNoteAsync_ShouldThrowException_WhenNoteDoesNotExist()
        {
            // Arrange
            var noteId = _fixture.Create<string>();

            _noteRepositoryMock.Setup(x => x.GetNoteByIdAsync(noteId))
                .ReturnsAsync((Note)null);

            // Act
            Func<Task> act = async () => await _noteService.DeleteNoteAsync(noteId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ResponseMessages.NoteNotFound);
        }


        [Fact]
        public async Task GetNotesForUserAsync_ShouldThrowException_WhenSortByIsInvalid()
        {
            // Arrange
            var sortBy = _fixture.Create<string>();
            var userId = _fixture.Create<string>();
            var parameters = _fixture.Build<DataQueryParameters>()
                .With(p => p.SortBy, sortBy)
                .Create();

            // Act
            Func<Task> act = async () => await _noteService.GetNotesForUserAsync(userId, parameters);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage(string.Format(ResponseMessages.InvalidSortByColumn, sortBy));
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldThrowException_WhenFilterColumnIsInvalid()
        {
            // Arrange          
            var userId = _fixture.Create<string>();
            var invalidColumn = _fixture.Create<string>();
            var parameters = _fixture.Build<DataQueryParameters>()
                .With(p => p.SortBy, nameof(Note.Description))
                .With(p => p.FilterColumns, new string[] { invalidColumn })
                .Create();

            // Act
            Func<Task> act = async () => await _noteService.GetNotesForUserAsync(userId, parameters);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage(string.Format(ResponseMessages.InvalidFilterColumn, invalidColumn));
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldThrowException_WhenUserIsInvalid()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var parameters = _fixture.Build<DataQueryParameters>()
               .With(p => p.SortBy, nameof(Note.Status))
               .With(p => p.FilterColumns, new string[] { nameof(Note.Title) })
               .Create();

            _userRepositoryMock.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _noteService.GetNotesForUserAsync(userId, parameters);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(ResponseMessages.UserNotFound);
        }

        [Fact]
        public async Task GetNotesForUserAsync_ShouldReturnNotes_ForValidUserAndParameters()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var paginatedNotes = _fixture.Create<PaginationResult<Note>>();
            var expectedNotesDto = new PaginationResult<NoteDto>
            {
                Count = paginatedNotes.Count,
                Data = paginatedNotes.Data.Select(n => new NoteDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Description = n.Description,
                    Status = n.Status,
                    Priority = n.Priority,
                    UserId = n.UserId
                }).ToList()
            };
            var parameters = _fixture.Build<DataQueryParameters>()
                .With(p => p.SortBy, nameof(Note.Description))
                .With(p => p.FilterColumns, new string[] { nameof(Note.Description) })
                .Create();

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new User());
            _noteRepositoryMock.Setup(r => r.GetNotesForUserAsync(userId, parameters)).ReturnsAsync(paginatedNotes);

            // Act
            var result = await _noteService.GetNotesForUserAsync(userId, parameters);

            // Assert
            result.Should().BeEquivalentTo(expectedNotesDto);

        }

    }
}
