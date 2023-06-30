using NotesApp.Domain.Entities;

namespace NotesApp.Domain.RepositoryInterfaces
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetNotesForUserAsync(int userId);
        Task<Note> GetNoteByIdAsync(int noteId);
        Task AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(Note note);
    }
}
