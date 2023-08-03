using NotesApp.Application.Dto;
using NotesApp.Common.Models;

namespace NotesApp.Application.Services.Notes
{
    public interface INoteService
    {
        Task<IEnumerable<NoteDto>> GetNotesForUserAsync(string userId);
        Task<PaginationResult<NoteDto>> GetNotesForUserAsync(string userId, DataQueryParameters parameters);
        Task<NoteDto> AddNoteAsync(NoteDto note);
        Task<NoteDto> UpdateNoteAsync(NoteDto note);
        Task DeleteNoteAsync(string noteId);
    }
}
