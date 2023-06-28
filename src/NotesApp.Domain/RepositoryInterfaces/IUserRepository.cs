using NotesApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.Domain.RepositoryInterfaces
{
    public interface IUserRepository
    { 
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user); 
    }
}
