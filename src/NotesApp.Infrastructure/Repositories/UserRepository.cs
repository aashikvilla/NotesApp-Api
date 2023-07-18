using MongoDB.Bson;
using MongoDB.Driver;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;
namespace NotesApp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            // "Users" is the name of the collection in MongoDB
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Id, id);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Email, email);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }
    }
}
