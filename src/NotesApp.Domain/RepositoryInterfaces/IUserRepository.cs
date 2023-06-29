using NotesApp.Domain.Entities;

namespace NotesApp.Domain.RepositoryInterfaces
{
    public interface IUserRepository
    { 
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user); 
    }
}
