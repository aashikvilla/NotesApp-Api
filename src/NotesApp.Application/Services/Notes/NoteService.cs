using AutoMapper;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Common.Models;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;

namespace NotesApp.Application.Services.Notes
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public NoteService(INoteRepository noteRepository, IUserRepository userRepository, IMapper mapper)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }


        public async Task<IEnumerable<NoteDto>> GetNotesForUserAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(ResponseMessages.UserNotFound);
            }

            var notes = await _noteRepository.GetNotesForUserAsync(userId);

            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<NoteDto> AddNoteAsync(NoteDto noteDto)
        {
            var note = _mapper.Map<Note>(noteDto);

            if (note.Id != string.Empty)
            {
                throw new InvalidOperationException(ResponseMessages.NoteAlreadyExists);
            }

            await _noteRepository.AddNoteAsync(note);

            return _mapper.Map<NoteDto>(note);
        }


        public async Task<NoteDto> UpdateNoteAsync(NoteDto noteDto)
        {
            var note = _mapper.Map<Note>(noteDto);
            var existingNote = await _noteRepository.GetNoteByIdAsync(note.Id);

            if (existingNote == null)
            {
                throw new InvalidOperationException(ResponseMessages.NoteNotFound);
            }

            await _noteRepository.UpdateNoteAsync(note);

            return _mapper.Map<NoteDto>(note);
        }

        public async Task DeleteNoteAsync(string noteId)
        {
            var note = await _noteRepository.GetNoteByIdAsync(noteId);

            if (note == null)
            {
                throw new InvalidOperationException(ResponseMessages.NoteNotFound);
            }
            await _noteRepository.DeleteNoteAsync(note);
        }


        public async Task<PaginationResult<NoteDto>> GetNotesForUserAsync(string userId, DataQueryParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.SortBy) && !IsValidColumnOfNote(parameters.SortBy))
            {
                throw new ArgumentException(string.Format(ResponseMessages.InvalidSortByColumn, parameters.SortBy));
            }

            if (parameters.FilterColumns.Length > 0)
            {
                foreach (var column in parameters.FilterColumns)
                {
                    if (!IsValidColumnOfNote(column))
                    {
                        throw new ArgumentException(string.Format(ResponseMessages.InvalidFilterColumn, column));
                    }
                }
            }

            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(ResponseMessages.UserNotFound);
            }
            var result = await _noteRepository.GetNotesForUserAsync(userId, parameters);

            return new PaginationResult<NoteDto>
            {
                Data = _mapper.Map<List<NoteDto>>(result.Data),
                Count = result.Count
            };
        }

        private bool IsValidColumnOfNote(string propertyName)
        {
            var propertyNames = typeof(Note).GetProperties().Select(property => property.Name).ToList();

            return propertyNames.Contains(propertyName);
        }
    }
}
