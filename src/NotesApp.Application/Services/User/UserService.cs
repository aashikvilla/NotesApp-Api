using NotesApp.Application.Dto;
using NotesApp.Application.Response;

namespace NotesApp.Application.Services.User
{
    public class UserService : IUserService
    {
        public Task<ServiceResponse<UserDto>> RegisterUserAsync(UserRegisterDto request)
        {
            throw new NotImplementedException();
        }
    }

}
