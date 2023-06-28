using NotesApp.Domain.Entities;

namespace NotesApp.Infrastructure.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
