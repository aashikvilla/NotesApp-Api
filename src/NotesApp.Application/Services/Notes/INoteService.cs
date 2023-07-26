using NotesApp.Application.Dto;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.Services.Notes
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetNotesForUserAsync(string userId);
        Task<PaginationResult<Note>> GetNotesForUserAsync(string userId,int pageSize,int pageNumber,string searchTerm);
        Task<Note> AddNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(string noteId);
    }
}
