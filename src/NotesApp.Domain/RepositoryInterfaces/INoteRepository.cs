using NotesApp.Domain.Entities;

namespace NotesApp.Domain.RepositoryInterfaces
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetNotesForUserAsync(int userId);
        Task<Note> GetNoteByIdAsync(int noteId);
        Task<Note> AddNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Note note);
        Task<bool> DeleteNoteAsync(int noteId);
    }
}
