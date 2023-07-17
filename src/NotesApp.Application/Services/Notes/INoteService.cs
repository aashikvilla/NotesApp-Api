using NotesApp.Domain.Entities;

namespace NotesApp.Application.Services.Notes
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetNotesForUserAsync(int userId);
        Task<Note> AddNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int noteId);
    }
}
