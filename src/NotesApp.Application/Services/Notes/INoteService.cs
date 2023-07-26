using NotesApp.Common.Models;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.Services.Notes
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetNotesForUserAsync(string userId);
        Task<PaginationResult<Note>> GetNotesForUserAsync(string userId,DataQueryParameters parameters);
        Task<Note> AddNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(string noteId);
    }
}
