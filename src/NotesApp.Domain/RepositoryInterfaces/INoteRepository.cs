using NotesApp.Domain.Entities;

namespace NotesApp.Domain.RepositoryInterfaces
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetNotesForUserAsync(string userId);
        Task<Note> GetNoteByIdAsync(string noteId);
        Task AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(Note note);
        Task<IEnumerable<Note>> GetNotesForUserAsync(string userId,int pageSize,int pageNumber, string seachTerm);
        Task<long> GetNoteCountForUserAsync(string userId, string seachTerm);
    }
}
