using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Common.Models;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;

namespace NotesApp.Application.Services.Notes
{
    public class NoteService:INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;

        public NoteService(INoteRepository noteRepository,IUserRepository userRepository)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
        }


        public async Task<IEnumerable<Note>> GetNotesForUserAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(ResponseMessages.UserNotFound);
            }

            return await _noteRepository.GetNotesForUserAsync(userId);
        }

        public async Task<Note> AddNoteAsync(Note note)
        {
            if (note.Id != string.Empty)
            {
                throw new InvalidOperationException(ResponseMessages.NoteAlreadyExists);
            }
            await _noteRepository.AddNoteAsync(note);
            return note;
        }

        public async Task<Note> UpdateNoteAsync(Note note)
        {
            var existingNote = await _noteRepository.GetNoteByIdAsync(note.Id);

            if (existingNote == null)
            {
                throw new InvalidOperationException(ResponseMessages.NoteNotFound);
            }
            await _noteRepository.UpdateNoteAsync(note);

            return note;
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


        public async Task<PaginationResult<Note>> GetNotesForUserAsync(string userId, DataQueryParameters parameters)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(ResponseMessages.UserNotFound);
            }
            return await _noteRepository.GetNotesForUserAsync(userId, parameters);
        }
    }
}
